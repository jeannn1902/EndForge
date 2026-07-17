using EndForge.Models;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EndForge.Services;

public sealed class ProgresoCursoService {
    private readonly string carpetaDatos;
    private readonly string nombreMutex;
    private readonly JsonSerializerOptions opcionesJson;

    public string RutaProgreso { get; }

    public ProgresoCursoService()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EndForge")) {
    }

    internal ProgresoCursoService(string carpetaDatos) {
        this.carpetaDatos = carpetaDatos;
        RutaProgreso = Path.Combine(carpetaDatos, "progreso.json");
        nombreMutex = @"Local\EndForge.ProgresoCurso." + Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(RutaProgreso)));
        opcionesJson = CrearOpcionesJson();
    }

    public ResultadoCargaProgreso CargarProgreso() {
        string contenido;

        try {
            contenido = File.ReadAllText(RutaProgreso, Encoding.UTF8);
        } catch (FileNotFoundException ex) {
            return CrearResultadoCarga(EstadoCargaProgreso.ArchivoInexistente, error: ex);
        } catch (DirectoryNotFoundException ex) {
            return CrearResultadoCarga(EstadoCargaProgreso.ArchivoInexistente, error: ex);
        } catch (UnauthorizedAccessException ex) {
            return CrearResultadoCarga(EstadoCargaProgreso.PermisosInsuficientes, error: ex);
        } catch (SecurityException ex) {
            return CrearResultadoCarga(EstadoCargaProgreso.PermisosInsuficientes, error: ex);
        } catch (IOException ex) {
            return CrearResultadoCarga(EstadoCargaProgreso.ErrorIo, error: ex);
        } catch (Exception ex) {
            return CrearResultadoCarga(EstadoCargaProgreso.ErrorIo, error: ex);
        }

        if (string.IsNullOrWhiteSpace(contenido)) {
            return CrearResultadoCarga(EstadoCargaProgreso.ArchivoVacio);
        }

        try {
            using JsonDocument documento = JsonDocument.Parse(contenido);
            return LeerDocumento(documento.RootElement);
        } catch (JsonException ex) {
            return CrearResultadoCarga(
                EstadoCargaProgreso.ContenidoInvalido,
                registrosInvalidos: 1,
                error: ex);
        } catch (Exception ex) {
            return CrearResultadoCarga(
                EstadoCargaProgreso.ContenidoInvalido,
                registrosInvalidos: 1,
                error: ex);
        }
    }

    public ProgresoPractica? ObtenerPractica(string practicaId) {
        ResultadoCargaProgreso resultado = CargarProgreso();

        if (!resultado.DatosDisponibles || string.IsNullOrWhiteSpace(practicaId)) {
            return null;
        }

        return resultado.Progreso.Practicas.FirstOrDefault(progreso =>
            progreso.PracticaId.Equals(practicaId, StringComparison.OrdinalIgnoreCase));
    }

    public ResultadoEscrituraProgreso ActualizarEstado(
        string practicaId,
        EstadoPracticaCurso estado,
        string? rutaProyecto = null) {
        if (string.IsNullOrWhiteSpace(practicaId) || !Enum.IsDefined(estado)) {
            return CrearResultadoEscritura(EstadoEscrituraProgreso.ContenidoInvalido);
        }

        return EjecutarEscrituraConBloqueo(
            () => ActualizarEstadoSinBloqueo(practicaId, estado, rutaProyecto));
    }

    private ResultadoEscrituraProgreso ActualizarEstadoSinBloqueo(
        string practicaId,
        EstadoPracticaCurso estado,
        string? rutaProyecto) {

        ResultadoCargaProgreso carga = CargarProgreso();
        ResultadoEscrituraProgreso? errorCarga = ConvertirErrorCarga(carga);

        if (errorCarga is not null) {
            return errorCarga;
        }

        ProgresoPractica? practica = carga.Progreso.Practicas.FirstOrDefault(item =>
            item.PracticaId.Equals(practicaId, StringComparison.OrdinalIgnoreCase));
        DateTimeOffset ahora = DateTimeOffset.Now;

        if (practica is null) {
            practica = new ProgresoPractica {
                PracticaId = practicaId.Trim(),
                FechaCreacion = ahora,
                FechaActualizacion = ahora
            };
            carga.Progreso.Practicas.Add(practica);
        }

        practica.Estado = estado;
        practica.FechaActualizacion = ahora;

        if (!string.IsNullOrWhiteSpace(rutaProyecto)) {
            practica.RutaProyecto = rutaProyecto.Trim();
        }

        practica.FechaFinalizacion = estado == EstadoPracticaCurso.Realizada
            ? ahora
            : null;

        return GuardarProgreso(carga.Progreso, carga.RegistrosInvalidos);
    }

    public ResultadoEscrituraProgreso ActualizarPractica(ProgresoPractica practica) {
        if (!EsRegistroValido(practica)) {
            return CrearResultadoEscritura(EstadoEscrituraProgreso.ContenidoInvalido);
        }

        return EjecutarEscrituraConBloqueo(
            () => ActualizarPracticaSinBloqueo(practica));
    }

    private ResultadoEscrituraProgreso ActualizarPracticaSinBloqueo(
        ProgresoPractica practica) {

        ResultadoCargaProgreso carga = CargarProgreso();
        ResultadoEscrituraProgreso? errorCarga = ConvertirErrorCarga(carga);

        if (errorCarga is not null) {
            return errorCarga;
        }

        carga.Progreso.Practicas.RemoveAll(item =>
            item.PracticaId.Equals(practica.PracticaId, StringComparison.OrdinalIgnoreCase));
        carga.Progreso.Practicas.Add(CopiarPractica(practica));

        return GuardarProgreso(carga.Progreso, carga.RegistrosInvalidos);
    }

    public ResultadoEscrituraProgreso GuardarProgreso(ProgresoCurso progreso) {
        return EjecutarEscrituraConBloqueo(() => GuardarProgreso(progreso, 0));
    }

    private ResultadoEscrituraProgreso EjecutarEscrituraConBloqueo(
        Func<ResultadoEscrituraProgreso> operacion) {
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
                return CrearResultadoEscritura(
                    EstadoEscrituraProgreso.ErrorIo,
                    error: new TimeoutException(
                        "Otra instancia de EndForge está actualizando progreso.json."));
            }

            return operacion();
        } catch (UnauthorizedAccessException ex) {
            return CrearResultadoEscritura(
                EstadoEscrituraProgreso.PermisosInsuficientes,
                error: ex);
        } catch (SecurityException ex) {
            return CrearResultadoEscritura(
                EstadoEscrituraProgreso.PermisosInsuficientes,
                error: ex);
        } catch (Exception ex) {
            return CrearResultadoEscritura(
                EstadoEscrituraProgreso.ErrorIo,
                error: ex);
        } finally {
            if (bloqueoAdquirido && mutex is not null) {
                try {
                    mutex.ReleaseMutex();
                } catch (Exception) {
                    // La operación principal ya terminó; la liberación no debe ocultar su resultado.
                }
            }

            mutex?.Dispose();
        }
    }

    private ResultadoCargaProgreso LeerDocumento(JsonElement raiz) {
        if (raiz.ValueKind != JsonValueKind.Object ||
            !IntentarObtenerPropiedad(raiz, nameof(ProgresoCurso.Practicas), out JsonElement practicasJson) ||
            practicasJson.ValueKind != JsonValueKind.Array) {
            return CrearResultadoCarga(
                EstadoCargaProgreso.ContenidoInvalido,
                registrosInvalidos: 1);
        }

        List<ProgresoPractica> practicas = new();
        HashSet<string> identificadores = new(StringComparer.OrdinalIgnoreCase);
        int registrosInvalidos = 0;

        foreach (JsonElement elemento in practicasJson.EnumerateArray()) {
            try {
                ProgresoPractica? practica = elemento.Deserialize<ProgresoPractica>(opcionesJson);

                if (practica is null ||
                    !EsRegistroValido(practica) ||
                    !identificadores.Add(practica.PracticaId)) {
                    registrosInvalidos++;
                    continue;
                }

                practicas.Add(practica);
            } catch (Exception) {
                registrosInvalidos++;
            }
        }

        ProgresoCurso progreso = new() {
            Practicas = practicas
        };
        EstadoCargaProgreso estado = registrosInvalidos > 0
            ? EstadoCargaProgreso.ContenidoInvalido
            : EstadoCargaProgreso.Exitosa;

        return CrearResultadoCarga(estado, progreso, registrosInvalidos);
    }

    private ResultadoEscrituraProgreso GuardarProgreso(
        ProgresoCurso progreso,
        int registrosInvalidosIgnorados) {
        if (!IntentarNormalizarProgreso(progreso, out ProgresoCurso progresoNormalizado)) {
            return CrearResultadoEscritura(EstadoEscrituraProgreso.ContenidoInvalido);
        }

        string? rutaTemporal = null;

        try {
            Directory.CreateDirectory(carpetaDatos);
            rutaTemporal = Path.Combine(
                carpetaDatos,
                $".progreso-{Guid.NewGuid():N}.tmp");
            string contenido = JsonSerializer.Serialize(progresoNormalizado, opcionesJson);
            File.WriteAllText(rutaTemporal, contenido, new UTF8Encoding(false));

            if (File.Exists(RutaProgreso)) {
                File.Replace(rutaTemporal, RutaProgreso, null);
            } else {
                File.Move(rutaTemporal, RutaProgreso);
            }

            return new ResultadoEscrituraProgreso {
                Estado = EstadoEscrituraProgreso.Exitosa,
                RegistrosInvalidosIgnorados = registrosInvalidosIgnorados
            };
        } catch (UnauthorizedAccessException ex) {
            LimpiarTemporal(rutaTemporal);
            return CrearResultadoEscritura(
                EstadoEscrituraProgreso.PermisosInsuficientes,
                registrosInvalidosIgnorados,
                ex);
        } catch (SecurityException ex) {
            LimpiarTemporal(rutaTemporal);
            return CrearResultadoEscritura(
                EstadoEscrituraProgreso.PermisosInsuficientes,
                registrosInvalidosIgnorados,
                ex);
        } catch (IOException ex) {
            LimpiarTemporal(rutaTemporal);
            return CrearResultadoEscritura(
                EstadoEscrituraProgreso.ErrorIo,
                registrosInvalidosIgnorados,
                ex);
        } catch (Exception ex) {
            LimpiarTemporal(rutaTemporal);
            return CrearResultadoEscritura(
                EstadoEscrituraProgreso.ErrorIo,
                registrosInvalidosIgnorados,
                ex);
        }
    }

    private static ResultadoEscrituraProgreso? ConvertirErrorCarga(
        ResultadoCargaProgreso carga) {
        return carga.Estado switch {
            EstadoCargaProgreso.PermisosInsuficientes => CrearResultadoEscritura(
                EstadoEscrituraProgreso.PermisosInsuficientes,
                carga.RegistrosInvalidos,
                carga.Error),
            EstadoCargaProgreso.ErrorIo => CrearResultadoEscritura(
                EstadoEscrituraProgreso.ErrorIo,
                carga.RegistrosInvalidos,
                carga.Error),
            EstadoCargaProgreso.ContenidoInvalido when carga.Progreso.Practicas.Count == 0 =>
                CrearResultadoEscritura(
                    EstadoEscrituraProgreso.ContenidoInvalido,
                    carga.RegistrosInvalidos,
                    carga.Error),
            _ => null
        };
    }

    private static bool IntentarNormalizarProgreso(
        ProgresoCurso? progreso,
        out ProgresoCurso progresoNormalizado) {
        progresoNormalizado = new ProgresoCurso();

        if (progreso?.Practicas is null) {
            return false;
        }

        HashSet<string> identificadores = new(StringComparer.OrdinalIgnoreCase);

        foreach (ProgresoPractica practica in progreso.Practicas) {
            if (!EsRegistroValido(practica) || !identificadores.Add(practica.PracticaId)) {
                return false;
            }

            progresoNormalizado.Practicas.Add(CopiarPractica(practica));
        }

        progresoNormalizado.Practicas = progresoNormalizado.Practicas
            .OrderBy(practica => practica.PracticaId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(practica => practica.PracticaId, StringComparer.Ordinal)
            .ToList();

        return true;
    }

    private static bool EsRegistroValido(ProgresoPractica? practica) {
        if (practica is null ||
            string.IsNullOrWhiteSpace(practica.PracticaId) ||
            !Enum.IsDefined(practica.Estado) ||
            practica.FechaCreacion == default) {
            return false;
        }

        if (practica.FechaFinalizacion < practica.FechaCreacion) {
            return false;
        }

        if (practica.FechaActualizacion < practica.FechaCreacion) {
            return false;
        }

        if (string.IsNullOrWhiteSpace(practica.RutaProyecto)) {
            return true;
        }

        try {
            return Path.IsPathFullyQualified(practica.RutaProyecto) &&
                !string.IsNullOrWhiteSpace(Path.GetFullPath(practica.RutaProyecto));
        } catch (Exception) {
            return false;
        }
    }

    private static ProgresoPractica CopiarPractica(ProgresoPractica practica) {
        return new ProgresoPractica {
            PracticaId = practica.PracticaId.Trim(),
            Estado = practica.Estado,
            RutaProyecto = (practica.RutaProyecto ?? string.Empty).Trim(),
            FechaCreacion = practica.FechaCreacion,
            FechaActualizacion = practica.FechaActualizacion,
            FechaFinalizacion = practica.FechaFinalizacion
        };
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

    private static ResultadoCargaProgreso CrearResultadoCarga(
        EstadoCargaProgreso estado,
        ProgresoCurso? progreso = null,
        int registrosInvalidos = 0,
        Exception? error = null) {
        return new ResultadoCargaProgreso {
            Estado = estado,
            Progreso = progreso ?? new ProgresoCurso(),
            RegistrosInvalidos = registrosInvalidos,
            Error = error
        };
    }

    private static ResultadoEscrituraProgreso CrearResultadoEscritura(
        EstadoEscrituraProgreso estado,
        int registrosInvalidosIgnorados = 0,
        Exception? error = null) {
        return new ResultadoEscrituraProgreso {
            Estado = estado,
            RegistrosInvalidosIgnorados = registrosInvalidosIgnorados,
            Error = error
        };
    }

    private static JsonSerializerOptions CrearOpcionesJson() {
        JsonSerializerOptions opciones = new() {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
        opciones.Converters.Add(new JsonStringEnumConverter());
        return opciones;
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
            // El error original de escritura tiene prioridad sobre la limpieza.
        }
    }
}
