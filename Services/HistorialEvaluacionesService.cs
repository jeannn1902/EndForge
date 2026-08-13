using EndForge.Models;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EndForge.Services;

public sealed class HistorialEvaluacionesService {
    private const int VersionActual = 1;
    private const int MaximoIntentosPorPractica = 50;
    private const int MaximoResultadosPorIntento = 100;
    private const int MaximoRetroalimentacionesPorIntento = 100;
    private const int MaximoCaracteresSalida = 262_144;
    private const int MaximoCaracteresRetroalimentacion = 8_192;
    private const long MaximoBytesArchivo = 128L * 1024L * 1024L;

    private readonly string carpetaDatos;
    private readonly string nombreMutex;
    private readonly JsonSerializerOptions opcionesJson;

    public string RutaEvaluaciones { get; }

    public HistorialEvaluacionesService()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EndForge")) {
    }

    internal HistorialEvaluacionesService(string carpetaDatos) {
        this.carpetaDatos = carpetaDatos;
        RutaEvaluaciones = Path.Combine(carpetaDatos, "evaluaciones.json");
        nombreMutex = @"Local\EndForge.HistorialEvaluaciones." + Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(RutaEvaluaciones)));
        opcionesJson = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    public ResultadoCargaHistorialEvaluaciones CargarHistorial() {
        string contenido;

        try {
            using FileStream archivo = new(
                RutaEvaluaciones,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            if (archivo.Length > MaximoBytesArchivo) {
                return CrearResultadoCarga(
                    EstadoCargaHistorialEvaluaciones.ContenidoIrrecuperable,
                    registrosInvalidos: 1,
                    error: new InvalidDataException(
                        "El historial de evaluaciones supera el tamaño máximo admitido."));
            }

            using StreamReader lector = new(
                archivo,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true);
            contenido = lector.ReadToEnd();
        } catch (FileNotFoundException ex) {
            return CrearResultadoCarga(
                EstadoCargaHistorialEvaluaciones.ArchivoInexistente,
                error: ex);
        } catch (DirectoryNotFoundException ex) {
            return CrearResultadoCarga(
                EstadoCargaHistorialEvaluaciones.ArchivoInexistente,
                error: ex);
        } catch (UnauthorizedAccessException ex) {
            return CrearResultadoCarga(
                EstadoCargaHistorialEvaluaciones.PermisosInsuficientes,
                error: ex);
        } catch (SecurityException ex) {
            return CrearResultadoCarga(
                EstadoCargaHistorialEvaluaciones.PermisosInsuficientes,
                error: ex);
        } catch (IOException ex) {
            return CrearResultadoCarga(
                EstadoCargaHistorialEvaluaciones.ErrorIo,
                error: ex);
        } catch (Exception ex) {
            return CrearResultadoCarga(
                EstadoCargaHistorialEvaluaciones.ErrorIo,
                error: ex);
        }

        if (string.IsNullOrWhiteSpace(contenido)) {
            return CrearResultadoCarga(EstadoCargaHistorialEvaluaciones.ArchivoVacio);
        }

        try {
            using JsonDocument documento = JsonDocument.Parse(contenido);
            return LeerDocumento(documento.RootElement);
        } catch (JsonException ex) {
            return CrearResultadoCarga(
                EstadoCargaHistorialEvaluaciones.ContenidoIrrecuperable,
                registrosInvalidos: 1,
                error: ex);
        } catch (Exception ex) {
            return CrearResultadoCarga(
                EstadoCargaHistorialEvaluaciones.ContenidoIrrecuperable,
                registrosInvalidos: 1,
                error: ex);
        }
    }

    public ResultadoEscrituraHistorialEvaluaciones GuardarIntento(
        IntentoPractica intento) {
        if (!IntentarNormalizarIntento(intento, out IntentoPractica normalizado)) {
            return new ResultadoEscrituraHistorialEvaluaciones {
                Estado = EstadoEscrituraHistorialEvaluaciones.IntentoInvalido
            };
        }

        return EjecutarConBloqueo(
            () => GuardarIntentoSinBloqueo(normalizado),
            error => CrearErrorEscritura(
                EstadoEscrituraHistorialEvaluaciones.PermisosInsuficientes,
                error),
            error => CrearErrorEscritura(
                EstadoEscrituraHistorialEvaluaciones.ErrorIo,
                error));
    }

    public ResultadoEliminacionHistorialEvaluaciones EliminarHistorialPractica(
        string practicaId) {
        if (string.IsNullOrWhiteSpace(practicaId)) {
            return new ResultadoEliminacionHistorialEvaluaciones {
                Estado = EstadoEliminacionHistorialEvaluaciones.IdentificadorInvalido
            };
        }

        string identificador = practicaId.Trim();
        return EjecutarConBloqueo(
            () => EliminarHistorialPracticaSinBloqueo(identificador),
            error => CrearErrorEliminacion(
                EstadoEliminacionHistorialEvaluaciones.PermisosInsuficientes,
                error),
            error => CrearErrorEliminacion(
                EstadoEliminacionHistorialEvaluaciones.ErrorIo,
                error));
    }

    private ResultadoEscrituraHistorialEvaluaciones GuardarIntentoSinBloqueo(
        IntentoPractica intento) {
        ResultadoCargaHistorialEvaluaciones carga = CargarHistorial();
        ResultadoEscrituraHistorialEvaluaciones? errorCarga =
            ConvertirErrorCargaAEscritura(carga);

        if (errorCarga is not null) {
            return errorCarga;
        }

        List<HistorialPracticaMutable> practicas = CrearHistorialMutable(carga.Historial);

        if (practicas.SelectMany(item => item.Intentos).Any(item =>
            item.Id.Equals(intento.Id, StringComparison.OrdinalIgnoreCase))) {
            return new ResultadoEscrituraHistorialEvaluaciones {
                Estado = EstadoEscrituraHistorialEvaluaciones.IntentoDuplicado,
                RegistrosInvalidosIgnorados = carga.RegistrosInvalidos
            };
        }

        HistorialPracticaMutable? practica = practicas.FirstOrDefault(item =>
            item.PracticaId.Equals(intento.PracticaId, StringComparison.OrdinalIgnoreCase));

        if (practica is null) {
            practica = new HistorialPracticaMutable {
                PracticaId = intento.PracticaId
            };
            practicas.Add(practica);
        }

        int? mejorCalificacionAnterior = practica.MejorCalificacion;
        int? ultimaCalificacionAnterior = practica.UltimaCalificacion;
        DateTimeOffset? fechaUltimoIntentoAnterior = practica.FechaUltimoIntento;

        try {
            practica.TotalIntentos = checked(practica.TotalIntentos + 1);
        } catch (OverflowException ex) {
            return CrearErrorEscritura(
                EstadoEscrituraHistorialEvaluaciones.IntentoInvalido,
                ex,
                carga.RegistrosInvalidos);
        }

        practica.MejorCalificacion = practica.MejorCalificacion.HasValue
            ? Math.Max(practica.MejorCalificacion.Value, intento.Calificacion)
            : intento.Calificacion;
        practica.UltimaCalificacion = intento.Calificacion;
        practica.FechaUltimoIntento = intento.Fecha;
        practica.Intentos.Add(CopiarIntento(intento));
        practica.Intentos = OrdenarYLimitarIntentos(
            practica.Intentos,
            intento.Id);

        EscribirHistorial(practicas);
        HistorialPractica historialActualizado = ConvertirAPublico(practica);
        IntentoPractica intentoPublicado = historialActualizado.Intentos.Single(item =>
            item.Id.Equals(intento.Id, StringComparison.OrdinalIgnoreCase));
        TransicionEvaluacionPersistida transicionPersistida =
            new() {
                PracticaId = intentoPublicado.PracticaId,
                IntentoId = intentoPublicado.Id,
                FechaIntento = intentoPublicado.Fecha,
                CalificacionIntento = intentoPublicado.Calificacion,
                MejorCalificacionAnterior = mejorCalificacionAnterior,
                UltimaCalificacionAnterior = ultimaCalificacionAnterior,
                FechaUltimoIntentoAnterior = fechaUltimoIntentoAnterior,
                MejorCalificacionPosterior = practica.MejorCalificacion
                    ?? intentoPublicado.Calificacion,
                TotalIntentos = practica.TotalIntentos,
                IntentoPublicado = true
            };

        return new ResultadoEscrituraHistorialEvaluaciones {
            Estado = EstadoEscrituraHistorialEvaluaciones.Exitosa,
            HistorialActualizado = historialActualizado,
            TransicionPersistida = transicionPersistida,
            RegistrosInvalidosIgnorados = carga.RegistrosInvalidos
        };
    }

    private ResultadoEliminacionHistorialEvaluaciones
        EliminarHistorialPracticaSinBloqueo(string practicaId) {
        ResultadoCargaHistorialEvaluaciones carga = CargarHistorial();
        ResultadoEliminacionHistorialEvaluaciones? errorCarga =
            ConvertirErrorCargaAEliminacion(carga);

        if (errorCarga is not null) {
            return errorCarga;
        }

        List<HistorialPracticaMutable> practicas = CrearHistorialMutable(carga.Historial);
        int eliminados = practicas.RemoveAll(item =>
            item.PracticaId.Equals(practicaId, StringComparison.OrdinalIgnoreCase));

        if (eliminados == 0) {
            return new ResultadoEliminacionHistorialEvaluaciones {
                Estado = EstadoEliminacionHistorialEvaluaciones.HistorialInexistente,
                RegistrosInvalidosIgnorados = carga.RegistrosInvalidos
            };
        }

        EscribirHistorial(practicas);
        return new ResultadoEliminacionHistorialEvaluaciones {
            Estado = EstadoEliminacionHistorialEvaluaciones.Exitosa,
            RegistrosInvalidosIgnorados = carga.RegistrosInvalidos
        };
    }

    private ResultadoCargaHistorialEvaluaciones LeerDocumento(JsonElement raiz) {
        if (raiz.ValueKind != JsonValueKind.Object) {
            return CrearResultadoCarga(
                EstadoCargaHistorialEvaluaciones.ContenidoIrrecuperable,
                registrosInvalidos: 1);
        }

        int registrosInvalidos = 0;
        int version = VersionActual;

        if (IntentarObtenerPropiedad(raiz, nameof(HistorialEvaluaciones.Version), out JsonElement versionJson)) {
            if (versionJson.ValueKind != JsonValueKind.Number ||
                !versionJson.TryGetInt32(out version)) {
                return CrearResultadoCarga(
                    EstadoCargaHistorialEvaluaciones.ContenidoIrrecuperable,
                    registrosInvalidos: 1);
            }

            if (version != VersionActual) {
                return CrearResultadoCarga(
                    EstadoCargaHistorialEvaluaciones.VersionNoCompatible);
            }
        } else {
            registrosInvalidos++;
        }

        if (!IntentarObtenerPropiedad(
                raiz,
                nameof(HistorialEvaluaciones.Practicas),
                out JsonElement practicasJson) ||
            practicasJson.ValueKind != JsonValueKind.Array) {
            return CrearResultadoCarga(
                EstadoCargaHistorialEvaluaciones.ContenidoIrrecuperable,
                registrosInvalidos: registrosInvalidos + 1);
        }

        Dictionary<string, HistorialPracticaMutable> practicas =
            new(StringComparer.OrdinalIgnoreCase);

        foreach (JsonElement practicaJson in practicasJson.EnumerateArray()) {
            if (!IntentarLeerPractica(
                    practicaJson,
                    out HistorialPracticaMutable practica,
                    out int invalidosPractica)) {
                registrosInvalidos += Math.Max(1, invalidosPractica);
                continue;
            }

            registrosInvalidos += invalidosPractica;

            if (practicas.TryGetValue(practica.PracticaId, out HistorialPracticaMutable? existente)) {
                registrosInvalidos++;
                FusionarPracticas(existente, practica, ref registrosInvalidos);
            } else {
                practicas.Add(practica.PracticaId, practica);
            }
        }

        if (practicasJson.GetArrayLength() > 0 && practicas.Count == 0) {
            return CrearResultadoCarga(
                EstadoCargaHistorialEvaluaciones.ContenidoIrrecuperable,
                registrosInvalidos: Math.Max(1, registrosInvalidos));
        }

        HistorialEvaluaciones historial = ConvertirAPublico(practicas.Values);
        EstadoCargaHistorialEvaluaciones estado = registrosInvalidos > 0
            ? EstadoCargaHistorialEvaluaciones.ContenidoParcialmenteInvalido
            : EstadoCargaHistorialEvaluaciones.Exitosa;
        return CrearResultadoCarga(estado, historial, registrosInvalidos);
    }

    private bool IntentarLeerPractica(
        JsonElement elemento,
        out HistorialPracticaMutable practica,
        out int registrosInvalidos) {
        practica = new HistorialPracticaMutable();
        registrosInvalidos = 0;

        if (elemento.ValueKind != JsonValueKind.Object ||
            !IntentarLeerCadena(elemento, nameof(HistorialPractica.PracticaId), out string practicaId) ||
            string.IsNullOrWhiteSpace(practicaId)) {
            registrosInvalidos = 1;
            return false;
        }

        practica.PracticaId = practicaId.Trim();

        if (!IntentarLeerEnteroNoNegativo(
                elemento,
                nameof(HistorialPractica.TotalIntentos),
                out int totalIntentos)) {
            registrosInvalidos++;
            totalIntentos = 0;
        }

        practica.TotalIntentos = totalIntentos;

        if (!IntentarLeerCalificacionOpcional(
                elemento,
                nameof(HistorialPractica.MejorCalificacion),
                out int? mejorCalificacion)) {
            registrosInvalidos++;
        }

        if (!IntentarLeerCalificacionOpcional(
                elemento,
                nameof(HistorialPractica.UltimaCalificacion),
                out int? ultimaCalificacion)) {
            registrosInvalidos++;
        }

        if (!IntentarLeerFechaOpcional(
                elemento,
                nameof(HistorialPractica.FechaUltimoIntento),
                out DateTimeOffset? fechaUltimoIntento)) {
            registrosInvalidos++;
        }

        practica.MejorCalificacion = mejorCalificacion;
        practica.UltimaCalificacion = ultimaCalificacion;
        practica.FechaUltimoIntento = fechaUltimoIntento;

        HashSet<string> idsIntentos = new(StringComparer.OrdinalIgnoreCase);

        if (IntentarObtenerPropiedad(elemento, nameof(HistorialPractica.Intentos), out JsonElement intentosJson)) {
            if (intentosJson.ValueKind != JsonValueKind.Array) {
                registrosInvalidos++;
            } else {
                foreach (JsonElement intentoJson in intentosJson.EnumerateArray()) {
                    IntentoPractica? intento;

                    try {
                        intento = intentoJson.Deserialize<IntentoPractica>(opcionesJson);
                    } catch (Exception) {
                        registrosInvalidos++;
                        continue;
                    }

                    if (!IntentarNormalizarIntento(intento, out IntentoPractica normalizado) ||
                        !normalizado.PracticaId.Equals(
                            practica.PracticaId,
                            StringComparison.OrdinalIgnoreCase) ||
                        !idsIntentos.Add(normalizado.Id)) {
                        registrosInvalidos++;
                        continue;
                    }

                    practica.Intentos.Add(normalizado);
                }
            }
        } else {
            registrosInvalidos++;
        }

        int cantidadAntesLimite = practica.Intentos.Count;
        practica.Intentos = OrdenarYLimitarIntentos(practica.Intentos);
        registrosInvalidos += cantidadAntesLimite - practica.Intentos.Count;
        NormalizarResumenPractica(practica, ref registrosInvalidos);
        return true;
    }

    private static void NormalizarResumenPractica(
        HistorialPracticaMutable practica,
        ref int registrosInvalidos) {
        if (practica.TotalIntentos < practica.Intentos.Count) {
            practica.TotalIntentos = practica.Intentos.Count;
            registrosInvalidos++;
        }

        if (practica.TotalIntentos == 0) {
            if (practica.MejorCalificacion.HasValue ||
                practica.UltimaCalificacion.HasValue ||
                practica.FechaUltimoIntento.HasValue) {
                registrosInvalidos++;
            }

            practica.MejorCalificacion = null;
            practica.UltimaCalificacion = null;
            practica.FechaUltimoIntento = null;
            return;
        }

        if (practica.Intentos.Count == 0) {
            return;
        }

        int mejorConservada = practica.Intentos.Max(item => item.Calificacion);

        if (!practica.MejorCalificacion.HasValue ||
            practica.MejorCalificacion.Value < mejorConservada) {
            practica.MejorCalificacion = mejorConservada;
            registrosInvalidos++;
        }

        IntentoPractica intentoMasReciente = practica.Intentos
            .OrderByDescending(item => item.Fecha)
            .ThenByDescending(item => item.Id, StringComparer.Ordinal)
            .First();

        if (!practica.FechaUltimoIntento.HasValue ||
            practica.FechaUltimoIntento.Value < intentoMasReciente.Fecha) {
            practica.FechaUltimoIntento = intentoMasReciente.Fecha;
            practica.UltimaCalificacion = intentoMasReciente.Calificacion;
            registrosInvalidos++;
        } else if (practica.FechaUltimoIntento.Value == intentoMasReciente.Fecha &&
            practica.UltimaCalificacion != intentoMasReciente.Calificacion) {
            practica.UltimaCalificacion = intentoMasReciente.Calificacion;
            registrosInvalidos++;
        }
    }

    private static void FusionarPracticas(
        HistorialPracticaMutable destino,
        HistorialPracticaMutable origen,
        ref int registrosInvalidos) {
        destino.TotalIntentos = Math.Max(destino.TotalIntentos, origen.TotalIntentos);

        if (origen.MejorCalificacion.HasValue) {
            destino.MejorCalificacion = destino.MejorCalificacion.HasValue
                ? Math.Max(destino.MejorCalificacion.Value, origen.MejorCalificacion.Value)
                : origen.MejorCalificacion;
        }

        if (origen.FechaUltimoIntento.HasValue &&
            (!destino.FechaUltimoIntento.HasValue ||
             origen.FechaUltimoIntento.Value > destino.FechaUltimoIntento.Value)) {
            destino.FechaUltimoIntento = origen.FechaUltimoIntento;
            destino.UltimaCalificacion = origen.UltimaCalificacion;
        }

        HashSet<string> ids = destino.Intentos
            .Select(item => item.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (IntentoPractica intento in origen.Intentos) {
            if (ids.Add(intento.Id)) {
                destino.Intentos.Add(CopiarIntento(intento));
            } else {
                registrosInvalidos++;
            }
        }

        int cantidadAntesLimite = destino.Intentos.Count;
        destino.Intentos = OrdenarYLimitarIntentos(destino.Intentos);
        registrosInvalidos += cantidadAntesLimite - destino.Intentos.Count;
        NormalizarResumenPractica(destino, ref registrosInvalidos);
    }

    private void EscribirHistorial(IEnumerable<HistorialPracticaMutable> practicas) {
        string? rutaTemporal = null;

        try {
            Directory.CreateDirectory(carpetaDatos);
            rutaTemporal = Path.Combine(
                carpetaDatos,
                $".evaluaciones-{Guid.NewGuid():N}.tmp");
            ArchivoEvaluacionesDto archivo = new() {
                Version = VersionActual,
                Practicas = practicas
                    .OrderBy(item => item.PracticaId, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(item => item.PracticaId, StringComparer.Ordinal)
                    .Select(ConvertirADto)
                    .ToList()
            };
            string contenido = JsonSerializer.Serialize(archivo, opcionesJson);
            File.WriteAllText(rutaTemporal, contenido, new UTF8Encoding(false));

            if (File.Exists(RutaEvaluaciones)) {
                File.Replace(rutaTemporal, RutaEvaluaciones, null);
            } else {
                File.Move(rutaTemporal, RutaEvaluaciones);
            }

            rutaTemporal = null;
        } finally {
            LimpiarTemporal(rutaTemporal);
        }
    }

    private T EjecutarConBloqueo<T>(
        Func<T> operacion,
        Func<Exception, T> crearErrorPermisos,
        Func<Exception, T> crearErrorIo) {
        Mutex? mutex = null;
        bool bloqueoAdquirido = false;

        try {
            mutex = new Mutex(initiallyOwned: false, nombreMutex);

            try {
                bloqueoAdquirido = mutex.WaitOne(TimeSpan.FromSeconds(3));
            } catch (AbandonedMutexException) {
                bloqueoAdquirido = true;
            }

            if (!bloqueoAdquirido) {
                return crearErrorIo(new TimeoutException(
                    "Otra instancia de EndForge está actualizando evaluaciones.json."));
            }

            return operacion();
        } catch (UnauthorizedAccessException ex) {
            return crearErrorPermisos(ex);
        } catch (SecurityException ex) {
            return crearErrorPermisos(ex);
        } catch (Exception ex) {
            return crearErrorIo(ex);
        } finally {
            if (bloqueoAdquirido && mutex is not null) {
                try {
                    mutex.ReleaseMutex();
                } catch (Exception) {
                    // El resultado de la operación tiene prioridad sobre la liberación del mutex.
                }
            }

            mutex?.Dispose();
        }
    }

    private static ResultadoEscrituraHistorialEvaluaciones? ConvertirErrorCargaAEscritura(
        ResultadoCargaHistorialEvaluaciones carga) {
        return carga.Estado switch {
            EstadoCargaHistorialEvaluaciones.ContenidoIrrecuperable =>
                CrearErrorEscritura(
                    EstadoEscrituraHistorialEvaluaciones.ContenidoIrrecuperable,
                    carga.Error,
                    carga.RegistrosInvalidos),
            EstadoCargaHistorialEvaluaciones.VersionNoCompatible =>
                CrearErrorEscritura(
                    EstadoEscrituraHistorialEvaluaciones.VersionNoCompatible,
                    carga.Error,
                    carga.RegistrosInvalidos),
            EstadoCargaHistorialEvaluaciones.PermisosInsuficientes =>
                CrearErrorEscritura(
                    EstadoEscrituraHistorialEvaluaciones.PermisosInsuficientes,
                    carga.Error,
                    carga.RegistrosInvalidos),
            EstadoCargaHistorialEvaluaciones.ErrorIo =>
                CrearErrorEscritura(
                    EstadoEscrituraHistorialEvaluaciones.ErrorIo,
                    carga.Error,
                    carga.RegistrosInvalidos),
            _ => null
        };
    }

    private static ResultadoEliminacionHistorialEvaluaciones? ConvertirErrorCargaAEliminacion(
        ResultadoCargaHistorialEvaluaciones carga) {
        return carga.Estado switch {
            EstadoCargaHistorialEvaluaciones.ContenidoIrrecuperable =>
                CrearErrorEliminacion(
                    EstadoEliminacionHistorialEvaluaciones.ContenidoIrrecuperable,
                    carga.Error,
                    carga.RegistrosInvalidos),
            EstadoCargaHistorialEvaluaciones.VersionNoCompatible =>
                CrearErrorEliminacion(
                    EstadoEliminacionHistorialEvaluaciones.VersionNoCompatible,
                    carga.Error,
                    carga.RegistrosInvalidos),
            EstadoCargaHistorialEvaluaciones.PermisosInsuficientes =>
                CrearErrorEliminacion(
                    EstadoEliminacionHistorialEvaluaciones.PermisosInsuficientes,
                    carga.Error,
                    carga.RegistrosInvalidos),
            EstadoCargaHistorialEvaluaciones.ErrorIo =>
                CrearErrorEliminacion(
                    EstadoEliminacionHistorialEvaluaciones.ErrorIo,
                    carga.Error,
                    carga.RegistrosInvalidos),
            _ => null
        };
    }

    private static bool IntentarNormalizarIntento(
        IntentoPractica? intento,
        out IntentoPractica normalizado) {
        normalizado = new IntentoPractica();

        if (intento is null ||
            string.IsNullOrWhiteSpace(intento.Id) ||
            string.IsNullOrWhiteSpace(intento.PracticaId) ||
            intento.Fecha == default ||
            intento.Calificacion is < 0 or > 100 ||
            intento.PruebasTotales < 0 ||
            intento.PruebasSuperadas < 0 ||
            intento.PruebasSuperadas > intento.PruebasTotales ||
            string.IsNullOrWhiteSpace(intento.ResultadoGeneral) ||
            intento.PuntosMaximos <= 0 ||
            intento.PuntosObtenidos < 0 ||
            intento.PuntosObtenidos > intento.PuntosMaximos ||
            string.IsNullOrWhiteSpace(intento.RutaProyecto) ||
            intento.Resultados is null ||
            intento.Retroalimentacion is null ||
            intento.Resultados.Count > MaximoResultadosPorIntento ||
            intento.Retroalimentacion.Count > MaximoRetroalimentacionesPorIntento ||
            !IntentarNormalizarRuta(intento.RutaProyecto, out string rutaProyecto)) {
            return false;
        }

        List<ResultadoCasoPrueba> resultados = new();
        HashSet<string> casos = new(StringComparer.OrdinalIgnoreCase);

        foreach (ResultadoCasoPrueba resultado in intento.Resultados) {
            if (!IntentarCopiarResultado(resultado, out ResultadoCasoPrueba copia) ||
                !casos.Add(copia.CasoPruebaId)) {
                return false;
            }

            resultados.Add(copia);
        }

        List<string> retroalimentacion = new();

        foreach (string mensaje in intento.Retroalimentacion) {
            if (mensaje is null || mensaje.Length > MaximoCaracteresRetroalimentacion) {
                return false;
            }

            retroalimentacion.Add(mensaje);
        }

        normalizado = new IntentoPractica {
            Id = intento.Id.Trim(),
            PracticaId = intento.PracticaId.Trim(),
            Fecha = intento.Fecha,
            Calificacion = intento.Calificacion,
            Compilo = intento.Compilo,
            PruebasSuperadas = intento.PruebasSuperadas,
            PruebasTotales = intento.PruebasTotales,
            ResultadoGeneral = intento.ResultadoGeneral.Trim(),
            EjecucionFinalizada = intento.EjecucionFinalizada,
            PuntosObtenidos = intento.PuntosObtenidos,
            PuntosMaximos = intento.PuntosMaximos,
            RutaProyecto = rutaProyecto,
            Resultados = resultados.AsReadOnly(),
            Retroalimentacion = retroalimentacion.AsReadOnly()
        };
        return true;
    }

    private static bool IntentarCopiarResultado(
        ResultadoCasoPrueba? resultado,
        out ResultadoCasoPrueba copia) {
        copia = new ResultadoCasoPrueba();

        if (resultado is null ||
            string.IsNullOrWhiteSpace(resultado.CasoPruebaId) ||
            resultado.PuntosMaximos < 0 ||
            resultado.PuntosObtenidos < 0 ||
            resultado.PuntosObtenidos > resultado.PuntosMaximos ||
            (resultado.Entrada?.Length ?? 0) > MaximoCaracteresSalida ||
            (resultado.SalidaEsperada?.Length ?? 0) > MaximoCaracteresSalida ||
            (resultado.SalidaObtenida?.Length ?? 0) > MaximoCaracteresSalida ||
            (resultado.Mensaje?.Length ?? 0) > MaximoCaracteresRetroalimentacion) {
            return false;
        }

        copia = new ResultadoCasoPrueba {
            CasoPruebaId = resultado.CasoPruebaId.Trim(),
            Entrada = resultado.Entrada ?? string.Empty,
            SalidaEsperada = resultado.SalidaEsperada ?? string.Empty,
            SalidaObtenida = resultado.SalidaObtenida ?? string.Empty,
            Aprobado = resultado.Aprobado,
            PuntosObtenidos = resultado.PuntosObtenidos,
            PuntosMaximos = resultado.PuntosMaximos,
            Mensaje = resultado.Mensaje ?? string.Empty,
            EsVisible = resultado.EsVisible,
            EjecucionFinalizada = resultado.EjecucionFinalizada
        };
        return true;
    }

    private static bool IntentarNormalizarRuta(string ruta, out string rutaNormalizada) {
        rutaNormalizada = string.Empty;

        try {
            string rutaSinEspacios = ruta.Trim();

            if (!Path.IsPathFullyQualified(rutaSinEspacios)) {
                return false;
            }

            rutaNormalizada = Path.GetFullPath(rutaSinEspacios);
            return !string.IsNullOrWhiteSpace(rutaNormalizada);
        } catch (Exception) {
            return false;
        }
    }

    private static List<IntentoPractica> OrdenarYLimitarIntentos(
        IEnumerable<IntentoPractica> intentos,
        string? intentoPrioritarioId = null) {
        List<IntentoPractica> ordenados = intentos
            .OrderByDescending(item => item.Fecha)
            .ThenByDescending(item => item.Id, StringComparer.Ordinal)
            .Select(CopiarIntento)
            .ToList();

        if (string.IsNullOrWhiteSpace(intentoPrioritarioId) ||
            ordenados.Count <= MaximoIntentosPorPractica) {
            return ordenados.Take(MaximoIntentosPorPractica).ToList();
        }

        IntentoPractica? intentoPrioritario = ordenados.FirstOrDefault(item =>
            item.Id.Equals(intentoPrioritarioId, StringComparison.OrdinalIgnoreCase));

        if (intentoPrioritario is null) {
            return ordenados.Take(MaximoIntentosPorPractica).ToList();
        }

        return ordenados
            .Where(item => !item.Id.Equals(
                intentoPrioritarioId,
                StringComparison.OrdinalIgnoreCase))
            .Take(MaximoIntentosPorPractica - 1)
            .Append(intentoPrioritario)
            .OrderByDescending(item => item.Fecha)
            .ThenByDescending(item => item.Id, StringComparer.Ordinal)
            .Select(CopiarIntento)
            .ToList();
    }

    private static IntentoPractica CopiarIntento(IntentoPractica intento) {
        return new IntentoPractica {
            Id = intento.Id,
            PracticaId = intento.PracticaId,
            Fecha = intento.Fecha,
            Calificacion = intento.Calificacion,
            Compilo = intento.Compilo,
            PruebasSuperadas = intento.PruebasSuperadas,
            PruebasTotales = intento.PruebasTotales,
            ResultadoGeneral = intento.ResultadoGeneral,
            EjecucionFinalizada = intento.EjecucionFinalizada,
            PuntosObtenidos = intento.PuntosObtenidos,
            PuntosMaximos = intento.PuntosMaximos,
            RutaProyecto = intento.RutaProyecto,
            Resultados = intento.Resultados.Select(resultado => {
                IntentarCopiarResultado(resultado, out ResultadoCasoPrueba copia);
                return copia;
            }).ToArray(),
            Retroalimentacion = intento.Retroalimentacion.ToArray()
        };
    }

    private static HistorialEvaluaciones ConvertirAPublico(
        IEnumerable<HistorialPracticaMutable> practicas) {
        return new HistorialEvaluaciones {
            Version = VersionActual,
            Practicas = practicas
                .OrderBy(item => item.PracticaId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.PracticaId, StringComparer.Ordinal)
                .Select(ConvertirAPublico)
                .ToArray()
        };
    }

    private static HistorialPractica ConvertirAPublico(HistorialPracticaMutable practica) {
        return new HistorialPractica {
            PracticaId = practica.PracticaId,
            TotalIntentos = practica.TotalIntentos,
            MejorCalificacion = practica.MejorCalificacion,
            UltimaCalificacion = practica.UltimaCalificacion,
            FechaUltimoIntento = practica.FechaUltimoIntento,
            Intentos = practica.Intentos.Select(CopiarIntento).ToArray()
        };
    }

    private static List<HistorialPracticaMutable> CrearHistorialMutable(
        HistorialEvaluaciones historial) {
        return historial.Practicas.Select(practica => new HistorialPracticaMutable {
            PracticaId = practica.PracticaId,
            TotalIntentos = practica.TotalIntentos,
            MejorCalificacion = practica.MejorCalificacion,
            UltimaCalificacion = practica.UltimaCalificacion,
            FechaUltimoIntento = practica.FechaUltimoIntento,
            Intentos = practica.Intentos.Select(CopiarIntento).ToList()
        }).ToList();
    }

    private static HistorialPracticaDto ConvertirADto(HistorialPracticaMutable practica) {
        return new HistorialPracticaDto {
            PracticaId = practica.PracticaId,
            TotalIntentos = practica.TotalIntentos,
            MejorCalificacion = practica.MejorCalificacion,
            UltimaCalificacion = practica.UltimaCalificacion,
            FechaUltimoIntento = practica.FechaUltimoIntento,
            Intentos = practica.Intentos.Select(CopiarIntento).ToList()
        };
    }

    private static bool IntentarLeerCadena(
        JsonElement elemento,
        string nombre,
        out string valor) {
        valor = string.Empty;

        if (!IntentarObtenerPropiedad(elemento, nombre, out JsonElement propiedad) ||
            propiedad.ValueKind != JsonValueKind.String) {
            return false;
        }

        valor = propiedad.GetString() ?? string.Empty;
        return true;
    }

    private static bool IntentarLeerEnteroNoNegativo(
        JsonElement elemento,
        string nombre,
        out int valor) {
        valor = 0;
        return IntentarObtenerPropiedad(elemento, nombre, out JsonElement propiedad) &&
            propiedad.ValueKind == JsonValueKind.Number &&
            propiedad.TryGetInt32(out valor) &&
            valor >= 0;
    }

    private static bool IntentarLeerCalificacionOpcional(
        JsonElement elemento,
        string nombre,
        out int? valor) {
        valor = null;

        if (!IntentarObtenerPropiedad(elemento, nombre, out JsonElement propiedad) ||
            propiedad.ValueKind == JsonValueKind.Null) {
            return true;
        }

        if (propiedad.ValueKind != JsonValueKind.Number ||
            !propiedad.TryGetInt32(out int calificacion) ||
            calificacion is < 0 or > 100) {
            return false;
        }

        valor = calificacion;
        return true;
    }

    private static bool IntentarLeerFechaOpcional(
        JsonElement elemento,
        string nombre,
        out DateTimeOffset? valor) {
        valor = null;

        if (!IntentarObtenerPropiedad(elemento, nombre, out JsonElement propiedad) ||
            propiedad.ValueKind == JsonValueKind.Null) {
            return true;
        }

        if (propiedad.ValueKind != JsonValueKind.String ||
            !propiedad.TryGetDateTimeOffset(out DateTimeOffset fecha)) {
            return false;
        }

        valor = fecha;
        return true;
    }

    private static bool IntentarObtenerPropiedad(
        JsonElement elemento,
        string nombre,
        out JsonElement valor) {
        foreach (JsonProperty propiedad in elemento.EnumerateObject()) {
            if (propiedad.Name.Equals(nombre, StringComparison.OrdinalIgnoreCase)) {
                valor = propiedad.Value;
                return true;
            }
        }

        valor = default;
        return false;
    }

    private static ResultadoCargaHistorialEvaluaciones CrearResultadoCarga(
        EstadoCargaHistorialEvaluaciones estado,
        HistorialEvaluaciones? historial = null,
        int registrosInvalidos = 0,
        Exception? error = null) {
        return new ResultadoCargaHistorialEvaluaciones {
            Estado = estado,
            Historial = historial ?? new HistorialEvaluaciones(),
            RegistrosInvalidos = registrosInvalidos,
            Error = error
        };
    }

    private static ResultadoEscrituraHistorialEvaluaciones CrearErrorEscritura(
        EstadoEscrituraHistorialEvaluaciones estado,
        Exception? error,
        int registrosInvalidos = 0) {
        return new ResultadoEscrituraHistorialEvaluaciones {
            Estado = estado,
            RegistrosInvalidosIgnorados = registrosInvalidos,
            Error = error
        };
    }

    private static ResultadoEliminacionHistorialEvaluaciones CrearErrorEliminacion(
        EstadoEliminacionHistorialEvaluaciones estado,
        Exception? error,
        int registrosInvalidos = 0) {
        return new ResultadoEliminacionHistorialEvaluaciones {
            Estado = estado,
            RegistrosInvalidosIgnorados = registrosInvalidos,
            Error = error
        };
    }

    private static void LimpiarTemporal(string? rutaTemporal) {
        if (string.IsNullOrWhiteSpace(rutaTemporal)) {
            return;
        }

        try {
            if (File.Exists(rutaTemporal)) {
                File.Delete(rutaTemporal);
            }
        } catch (Exception) {
            // La limpieza no debe ocultar el error original de lectura o escritura.
        }
    }

    private sealed class HistorialPracticaMutable {
        public string PracticaId { get; set; } = string.Empty;
        public int TotalIntentos { get; set; }
        public int? MejorCalificacion { get; set; }
        public int? UltimaCalificacion { get; set; }
        public DateTimeOffset? FechaUltimoIntento { get; set; }
        public List<IntentoPractica> Intentos { get; set; } = new();
    }

    private sealed class ArchivoEvaluacionesDto {
        public int Version { get; set; } = VersionActual;
        public List<HistorialPracticaDto> Practicas { get; set; } = new();
    }

    private sealed class HistorialPracticaDto {
        public string PracticaId { get; set; } = string.Empty;
        public int TotalIntentos { get; set; }
        public int? MejorCalificacion { get; set; }
        public int? UltimaCalificacion { get; set; }
        public DateTimeOffset? FechaUltimoIntento { get; set; }
        public List<IntentoPractica> Intentos { get; set; } = new();
    }
}
