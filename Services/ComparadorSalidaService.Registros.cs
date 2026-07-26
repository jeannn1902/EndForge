using EndForge.Models;

namespace EndForge.Services;

public sealed partial class ComparadorSalidaService {
    private static ResultadoBloquesRegistroComparados CompararBloquesRegistro(
        string salida,
        ReglaBloquesRegistroEsperados regla) {
        IReadOnlyList<LineaEstructurada> lineas =
            SepararLineasEstructuradas(salida);
        List<BloqueRegistroEncontrado> bloques = ExtraerBloquesRegistro(
            lineas,
            regla,
            out bool textoNoPermitido);

        if (bloques.Count == 0) {
            bool coincideAusencia = !regla.Obligatoria;
            return new ResultadoBloquesRegistroComparados {
                Nombre = regla.Nombre,
                CantidadEsperada = regla.RegistrosEsperados.Count,
                CantidadEncontrada = 0,
                OrdenCorrecto = coincideAusencia,
                RegistrosFaltantes = coincideAusencia
                    ? Array.Empty<string>()
                    : regla.RegistrosEsperados
                        .Select(ObtenerNombreRegistro)
                        .ToArray(),
                Coincide = coincideAusencia,
                Mensaje = coincideAusencia
                    ? string.Empty
                    : CrearMensajeRegistros(
                        regla,
                        "no se encontró ningún bloque con una clave reconocida.")
            };
        }

        bool[] usados = new bool[bloques.Count];
        List<ResultadoRegistroComparado> resultados = new();
        List<string> faltantes = new();
        List<string> duplicados = new();
        List<int> orden = new();
        bool tieneContradiccion = textoNoPermitido;

        for (int indiceEsperado = 0;
             indiceEsperado < regla.RegistrosEsperados.Count;
             indiceEsperado++) {
            RegistroEsperado esperado = regla.RegistrosEsperados[indiceEsperado];
            List<int> candidatas = bloques
                .Select((bloque, indice) => (bloque, indice))
                .Where(item => CoincideClaveRegistro(
                    item.bloque.Clave,
                    esperado.Clave,
                    regla.TipoClave))
                .Select(item => item.indice)
                .ToList();

            if (candidatas.Count == 0) {
                faltantes.Add(ObtenerNombreRegistro(esperado));
                continue;
            }

            int seleccionada = candidatas.FirstOrDefault(indice => !usados[indice], -1);

            if (seleccionada < 0) {
                faltantes.Add(ObtenerNombreRegistro(esperado));
                continue;
            }

            usados[seleccionada] = true;
            orden.Add(seleccionada);
            ResultadoRegistroComparado resultado = CompararRegistro(
                bloques[seleccionada],
                esperado,
                regla);

            if (candidatas.Count > 1) {
                duplicados.Add(FormatearValorEstructurado(esperado.Clave));
                bool copiasCoherentes = candidatas.All(indice =>
                    CompararRegistro(
                        bloques[indice],
                        esperado,
                        regla).Coincide);
                tieneContradiccion |= !copiasCoherentes;

                foreach (int indiceDuplicado in candidatas) {
                    usados[indiceDuplicado] = true;
                }

                resultado = new ResultadoRegistroComparado {
                    Nombre = resultado.Nombre,
                    ClaveEsperada = resultado.ClaveEsperada,
                    ClaveEncontrada = resultado.ClaveEncontrada,
                    NumeroBloque = resultado.NumeroBloque,
                    Campos = resultado.Campos,
                    EsDuplicado = true,
                    Coincide = resultado.Coincide &&
                        regla.PermitirRegistrosDuplicados
                };
            }

            tieneContradiccion |= resultado.Campos.Any(campo =>
                campo.TieneContradiccion);
            resultados.Add(resultado);
        }

        List<string> adicionales = bloques
            .Select((bloque, indice) => (bloque, indice))
            .Where(item => !usados[item.indice])
            .Select(item => item.bloque.Clave)
            .ToList();
        bool ordenCorrecto = !regla.OrdenRegistrosObligatorio ||
            orden.SequenceEqual(orden.OrderBy(indice => indice));
        bool coincide =
            faltantes.Count == 0 &&
            (regla.PermitirRegistrosAdicionales || adicionales.Count == 0) &&
            (regla.PermitirRegistrosDuplicados || duplicados.Count == 0) &&
            resultados.All(resultado => resultado.Coincide) &&
            ordenCorrecto &&
            !tieneContradiccion;

        return new ResultadoBloquesRegistroComparados {
            Nombre = regla.Nombre,
            Registros = resultados.AsReadOnly(),
            CantidadEsperada = regla.RegistrosEsperados.Count,
            CantidadEncontrada = bloques.Count,
            OrdenCorrecto = ordenCorrecto,
            RegistrosFaltantes = faltantes.AsReadOnly(),
            RegistrosDuplicados = duplicados
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            RegistrosAdicionales = adicionales.AsReadOnly(),
            TieneContradiccion = tieneContradiccion,
            Coincide = coincide,
            Mensaje = coincide
                ? string.Empty
                : CrearMensajeRegistros(
                    regla,
                    ObtenerDetalleErrorRegistros(
                        faltantes,
                        adicionales,
                        duplicados,
                        resultados,
                        ordenCorrecto,
                        textoNoPermitido))
        };
    }

    private static List<BloqueRegistroEncontrado> ExtraerBloquesRegistro(
        IReadOnlyList<LineaEstructurada> lineas,
        ReglaBloquesRegistroEsperados regla,
        out bool textoNoPermitido) {
        List<BloqueRegistroEncontrado> bloques = new();
        List<LineaEstructurada> textoFuera = new();
        BloqueRegistroEncontrado? actual = null;

        foreach (LineaEstructurada linea in lineas) {
            if (IntentarExtraerTrasEtiqueta(
                linea.Texto,
                regla.EtiquetasClave,
                out string etiqueta,
                out string clave)) {
                actual = new BloqueRegistroEncontrado(
                    bloques.Count + 1,
                    etiqueta,
                    clave,
                    new List<LineaEstructurada>());
                bloques.Add(actual);
                continue;
            }

            if (string.IsNullOrWhiteSpace(linea.Texto)) {
                continue;
            }

            if (actual is null) {
                textoFuera.Add(linea);
            } else {
                actual.Lineas.Add(linea);
            }
        }

        textoNoPermitido = !regla.PermitirTextoNeutralEntreBloques &&
            (textoFuera.Count > 0 ||
             bloques.Any(bloque => bloque.Lineas.Any(linea =>
                 !EsCampoReconocido(linea.Texto, regla))));
        return bloques;
    }

    private static bool EsCampoReconocido(
        string linea,
        ReglaBloquesRegistroEsperados regla) {
        return regla.RegistrosEsperados
            .SelectMany(registro => registro.Campos)
            .SelectMany(campo => campo.EtiquetasAlternativas)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Any(etiqueta => IntentarExtraerTrasEtiqueta(
                linea,
                Array.AsReadOnly(new[] { etiqueta }),
                out _,
                out _));
    }

    private static bool CoincideClaveRegistro(
        string encontrada,
        ValorEstructuradoEsperado esperada,
        TipoValorEstructurado tipoClave) {
        return esperada.Tipo == tipoClave &&
            CoincideValorEstructurado(encontrada, esperada);
    }

    private static ResultadoRegistroComparado CompararRegistro(
        BloqueRegistroEncontrado encontrado,
        RegistroEsperado esperado,
        ReglaBloquesRegistroEsperados regla) {
        List<ResultadoCampoRegistroComparado> campos = esperado.Campos
            .Select(campo => CompararCampoRegistro(encontrado, campo))
            .ToList();

        return new ResultadoRegistroComparado {
            Nombre = ObtenerNombreRegistro(esperado),
            ClaveEsperada = FormatearValorEstructurado(esperado.Clave),
            ClaveEncontrada = FormatearValorEncontrado(
                regla.TipoClave,
                encontrado.Clave),
            NumeroBloque = encontrado.NumeroBloque,
            Campos = campos.AsReadOnly(),
            Coincide = campos.All(campo => campo.Coincide)
        };
    }

    private static ResultadoCampoRegistroComparado CompararCampoRegistro(
        BloqueRegistroEncontrado bloque,
        CampoRegistroEsperado campo) {
        List<(string Etiqueta, string Valor)> valores = new();

        foreach (LineaEstructurada linea in bloque.Lineas) {
            if (IntentarExtraerTrasEtiqueta(
                linea.Texto,
                campo.EtiquetasAlternativas,
                out string etiqueta,
                out string valor)) {
                valores.Add((etiqueta, valor));
            }
        }

        bool todosCoinciden = valores.All(item =>
            CoincideValorEstructurado(item.Valor, campo.Valor));
        bool tieneContradiccion = valores.Count > 1 &&
            !todosCoinciden &&
            valores
                .Select(item => NormalizarValorRegistro(
                    item.Valor,
                    campo.Valor))
                .Distinct(StringComparer.Ordinal)
                .Skip(1)
                .Any();
        bool coincide = valores.Count == 0
            ? !campo.Obligatorio
            : todosCoinciden &&
              !tieneContradiccion;

        return new ResultadoCampoRegistroComparado {
            Nombre = campo.Nombre,
            ValorEsperado = FormatearValorEstructurado(campo.Valor),
            ValoresEncontrados = valores
                .Select(item => FormatearValorEncontrado(
                    campo.Valor.Tipo,
                    item.Valor))
                .ToArray(),
            EtiquetaEncontrada = valores.FirstOrDefault().Etiqueta ??
                string.Empty,
            EsObligatorio = campo.Obligatorio,
            TieneContradiccion = tieneContradiccion,
            Coincide = coincide
        };
    }

    private static string NormalizarValorRegistro(
        string valor,
        ValorEstructuradoEsperado esperado) {
        if (esperado.Tipo == TipoValorEstructurado.Numerico &&
            IntentarConvertirNumeroEstructurado(
                ExtraerPrimerTokenNumerico(valor),
                out double numero)) {
            return numero.ToString("G15", System.Globalization.CultureInfo.InvariantCulture);
        }

        if (esperado.Tipo == TipoValorEstructurado.Booleano &&
            IntentarConvertirBooleanoEstructurado(
                valor,
                esperado,
                out bool booleano)) {
            return booleano ? "true" : "false";
        }

        return NormalizarCadenaEstructurada(
            valor,
            esperado.DistinguirMayusculas,
            esperado.DistinguirAcentos,
            esperado.PoliticaEspacios);
    }

    private static string ObtenerNombreRegistro(RegistroEsperado registro) {
        return !string.IsNullOrWhiteSpace(registro.Nombre)
            ? registro.Nombre
            : FormatearValorEstructurado(registro.Clave);
    }

    private static string ObtenerDetalleErrorRegistros(
        IReadOnlyList<string> faltantes,
        IReadOnlyList<string> adicionales,
        IReadOnlyList<string> duplicados,
        IReadOnlyList<ResultadoRegistroComparado> registros,
        bool ordenCorrecto,
        bool textoNoPermitido) {
        if (textoNoPermitido) {
            return "se encontró texto fuera de los campos configurados.";
        }

        if (faltantes.Count > 0) {
            return $"falta el registro {faltantes[0]}.";
        }

        if (adicionales.Count > 0) {
            return $"sobra el registro con clave {adicionales[0]}.";
        }

        if (duplicados.Count > 0) {
            return $"la clave {duplicados[0]} está duplicada.";
        }

        if (!ordenCorrecto) {
            return "los registros no aparecen en el orden esperado.";
        }

        ResultadoRegistroComparado? registroIncorrecto = registros
            .FirstOrDefault(registro => !registro.Coincide);
        ResultadoCampoRegistroComparado? campoIncorrecto =
            registroIncorrecto?.Campos.FirstOrDefault(campo => !campo.Coincide);

        if (registroIncorrecto is not null && campoIncorrecto is not null) {
            return campoIncorrecto.TieneContradiccion
                ? $"el campo {campoIncorrecto.Nombre} del registro {registroIncorrecto.ClaveEsperada} contiene valores contradictorios."
                : $"el campo {campoIncorrecto.Nombre} del registro {registroIncorrecto.ClaveEsperada} está ausente o no coincide.";
        }

        return "uno o más bloques no coinciden.";
    }

    private static string CrearMensajeRegistros(
        ReglaBloquesRegistroEsperados regla,
        string detalle) {
        return !string.IsNullOrWhiteSpace(regla.MensajeError)
            ? regla.MensajeError
            : $"En {regla.Nombre}, {detalle}";
    }

    private sealed record BloqueRegistroEncontrado(
        int NumeroBloque,
        string EtiquetaClave,
        string Clave,
        List<LineaEstructurada> Lineas);
}
