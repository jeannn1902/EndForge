using EndForge.Models;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace EndForge.Services;

public sealed partial class ComparadorSalidaService {
    public ResultadoComparacionSalida Comparar(CasoPrueba caso, string? salidaObtenida) {
        ArgumentNullException.ThrowIfNull(caso);

        string salida = salidaObtenida ?? string.Empty;
        string salidaNormalizada = NormalizarTexto(salida);
        bool compararTexto = caso.ModoComparacion is
            ModoComparacionCaso.Texto or ModoComparacionCaso.Mixto;
        bool compararValores = caso.ModoComparacion is
            ModoComparacionCaso.Valores or ModoComparacionCaso.Mixto;
        bool compararSecuencias = caso.ModoComparacion == ModoComparacionCaso.Secuencia ||
            caso.ModoComparacion == ModoComparacionCaso.Mixto &&
            (caso.SecuenciasEsperadas.Count > 0 ||
             caso.SecuenciasCompuestasEsperadas.Count > 0);

        List<string> tokensFaltantes = compararTexto
            ? caso.TokensObligatorios
                .Where(token => !ContieneToken(salidaNormalizada, token))
                .ToList()
            : new List<string>();

        List<string> gruposFaltantes = compararTexto
            ? caso.GruposTokensAlternativos
                .Where(grupo =>
                    grupo.Alternativas.Count == 0 ||
                    !CumpleGrupoAlternativo(salida, salidaNormalizada, grupo))
                .Select(grupo => grupo.Nombre)
                .ToList()
            : new List<string>();

        HashSet<PosicionNumero> numerosUtilizados = new();
        List<ResultadoValorNumericoComparado> valoresComparados = compararValores
            ? caso.ValoresNumericosEsperados
                .Select(valor => CompararValorNumerico(salida, valor, numerosUtilizados))
                .ToList()
            : new List<ResultadoValorNumericoComparado>();
        HashSet<PosicionNumero> booleanosUtilizados = new();
        List<ResultadoValorBooleanoComparado> booleanosComparados = compararValores
            ? caso.ValoresBooleanosEsperados
                .Select(valor => CompararValorBooleano(salida, valor, booleanosUtilizados))
                .ToList()
            : new List<ResultadoValorBooleanoComparado>();
        HashSet<PosicionNumero> textosUtilizados = new();
        List<ResultadoValorTextualComparado> textosComparados = compararTexto
            ? caso.ValoresTextualesEsperados
                .Select(valor => CompararValorTextual(salida, valor, textosUtilizados))
                .ToList()
            : new List<ResultadoValorTextualComparado>();
        List<ResultadoSecuenciaComparada> secuenciasComparadas = compararSecuencias
            ? caso.SecuenciasEsperadas
                .Select(secuencia => CompararSecuencia(salida, secuencia))
                .ToList()
            : new List<ResultadoSecuenciaComparada>();
        List<ResultadoSecuenciaCompuestaComparada> secuenciasCompuestasComparadas =
            compararSecuencias
                ? caso.SecuenciasCompuestasEsperadas
                    .Select(secuencia =>
                        CompararSecuenciaCompuesta(salida, secuencia))
                    .ToList()
                : new List<ResultadoSecuenciaCompuestaComparada>();

        bool cumpleTexto =
            tokensFaltantes.Count == 0 &&
            gruposFaltantes.Count == 0 &&
            textosComparados.All(valor => valor.Coincide);
        bool hayReglasValores =
            valoresComparados.Count > 0 || booleanosComparados.Count > 0;
        bool valoresCorrectos = caso.ModoComparacion switch {
            ModoComparacionCaso.Texto => true,
            ModoComparacionCaso.Secuencia => true,
            ModoComparacionCaso.Valores =>
                hayReglasValores &&
                valoresComparados.All(valor => valor.Coincide) &&
                booleanosComparados.All(valor => valor.Coincide),
            _ =>
                !hayReglasValores ||
                valoresComparados.All(valor => valor.Coincide) &&
                booleanosComparados.All(valor => valor.Coincide)
        };
        bool secuenciasCorrectas = !compararSecuencias ||
            secuenciasComparadas.Count + secuenciasCompuestasComparadas.Count > 0 &&
            secuenciasComparadas.All(secuencia => secuencia.Coincide) &&
            secuenciasCompuestasComparadas.All(secuencia => secuencia.Coincide);
        bool etiquetasValoresPresentes = !compararValores ||
            valoresComparados.All(valor =>
                valor.EsOpcional ||
                valor.DebeEstarAusente ||
                !string.IsNullOrWhiteSpace(valor.EtiquetaEncontrada)) &&
            booleanosComparados.All(valor => !string.IsNullOrWhiteSpace(valor.EtiquetaEncontrada));
        bool etiquetasTextoPresentes = !compararTexto ||
            textosComparados.All(valor =>
                valor.EsOpcional ||
                !string.IsNullOrWhiteSpace(valor.EtiquetaEncontrada));
        bool cumpleEstructura =
            cumpleTexto &&
            etiquetasValoresPresentes &&
            etiquetasTextoPresentes &&
            secuenciasCorrectas;
        List<string> contradicciones = valoresComparados
            .Where(valor => valor.TieneContradiccion)
            .Select(valor => valor.Nombre)
            .Concat(booleanosComparados
                .Where(valor => valor.TieneContradiccion)
                .Select(valor => valor.Nombre))
            .Concat(textosComparados
                .Where(valor => valor.TieneContradiccion)
                .Select(valor => valor.Nombre))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        List<string> etiquetasAlternativas = valoresComparados
            .Where(valor => valor.UsoEtiquetaAlternativa)
            .Select(valor => $"{valor.Nombre}: {valor.EtiquetaEncontrada}")
            .Concat(booleanosComparados
                .Where(valor => valor.UsoEtiquetaAlternativa)
                .Select(valor => $"{valor.Nombre}: {valor.EtiquetaEncontrada}"))
            .Concat(textosComparados
                .Where(valor => valor.UsoEtiquetaAlternativa)
                .Select(valor => $"{valor.Nombre}: {valor.EtiquetaEncontrada}"))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        List<string> reglasCumplidas = CrearReglasCumplidas(
            caso,
            compararTexto,
            compararValores,
            tokensFaltantes,
            gruposFaltantes,
            valoresComparados,
            booleanosComparados,
            textosComparados,
            secuenciasComparadas,
            secuenciasCompuestasComparadas);
        List<string> reglasIncumplidas = CrearReglasIncumplidas(
            caso,
            compararTexto,
            compararValores,
            tokensFaltantes,
            gruposFaltantes,
            valoresComparados,
            booleanosComparados,
            textosComparados,
            secuenciasComparadas,
            secuenciasCompuestasComparadas);
        bool salidaLegible = EsLegible(
            salida,
            compararTexto
                ? caso.TokensObligatorios.Count - tokensFaltantes.Count
                : 0,
            compararTexto ? caso.TokensObligatorios.Count : 0,
            secuenciasComparadas.Any(secuencia => secuencia.CantidadEncontrada > 0) ||
            secuenciasCompuestasComparadas.Any(secuencia =>
                secuencia.CantidadEncontrada > 0));
        bool esCorrecta =
            cumpleTexto &&
            valoresCorrectos &&
            secuenciasCorrectas &&
            contradicciones.Count == 0;

        return new ResultadoComparacionSalida {
            EsCorrecta = esCorrecta,
            CumpleEstructura = cumpleEstructura,
            EsSalidaLegible = salidaLegible,
            TokensFaltantes = tokensFaltantes.AsReadOnly(),
            GruposAlternativosFaltantes = gruposFaltantes.AsReadOnly(),
            ValoresNumericos = valoresComparados.AsReadOnly(),
            ValoresBooleanos = booleanosComparados.AsReadOnly(),
            ValoresTextuales = textosComparados.AsReadOnly(),
            Secuencias = secuenciasComparadas.AsReadOnly(),
            SecuenciasCompuestas = secuenciasCompuestasComparadas.AsReadOnly(),
            ReglasCumplidas = reglasCumplidas.AsReadOnly(),
            ReglasIncumplidas = reglasIncumplidas.AsReadOnly(),
            ContradiccionesDetectadas = contradicciones.AsReadOnly(),
            EtiquetasAlternativasReconocidas = etiquetasAlternativas.AsReadOnly(),
            Mensaje = CrearMensaje(
                salida,
                esCorrecta,
                cumpleEstructura,
                valoresComparados,
                booleanosComparados,
                textosComparados,
                secuenciasComparadas,
                secuenciasCompuestasComparadas,
                contradicciones)
        };
    }

    private static ResultadoSecuenciaComparada CompararSecuencia(
        string salida,
        SecuenciaEsperada esperada) {
        return esperada.Tipo switch {
            TipoSecuenciaEsperada.Numerica =>
                CompararSecuenciaNumerica(salida, esperada),
            TipoSecuenciaEsperada.Textual =>
                CompararSecuenciaTextual(salida, esperada),
            _ => CrearResultadoSecuenciaInvalida(
                esperada,
                "La secuencia tiene un tipo que EndForge no reconoce.")
        };
    }

    private static ResultadoSecuenciaComparada CompararSecuenciaNumerica(
        string salida,
        SecuenciaEsperada esperada) {
        IReadOnlyList<CandidatoSecuenciaNumerica> candidatos =
            ExtraerNumerosSecuencia(salida, esperada);
        double[] encontrados = candidatos
            .Select(candidato => candidato.Valor)
            .ToArray();
        double[] valoresEsperados = esperada.ValoresNumericosEsperados.ToArray();
        int cantidadEsperada = esperada.CantidadExacta ?? valoresEsperados.Length;
        bool cantidadCorrecta = esperada.PermitirElementosAdicionales
            ? encontrados.Length >= cantidadEsperada
            : encontrados.Length == cantidadEsperada;
        bool ordenCorrecto = !esperada.OrdenObligatorio ||
            CoincideOrdenNumerico(
                valoresEsperados,
                encontrados,
                esperada.ToleranciaNumerica,
                esperada.PermitirElementosAdicionales);
        int? primerIndiceDiferente = esperada.OrdenObligatorio
            ? EncontrarPrimerIndiceDiferente(
                valoresEsperados,
                encontrados,
                esperada.ToleranciaNumerica)
            : null;
        (IReadOnlyList<double> faltantes, IReadOnlyList<double> adicionales) =
            CalcularDiferenciasNumericas(
                valoresEsperados,
                encontrados,
                esperada.ToleranciaNumerica);
        IReadOnlyList<double> duplicados = esperada.PermitirDuplicados
            ? Array.Empty<double>()
            : EncontrarDuplicadosNumericos(
                encontrados,
                esperada.ToleranciaNumerica);
        IReadOnlyList<string> separadoresInvalidos =
            EncontrarSeparadoresNumericosInvalidos(salida, candidatos, esperada);
        List<string> elementosAdicionales = adicionales
            .Select(FormatearNumeroSecuencia)
            .ToList();
        elementosAdicionales.AddRange(separadoresInvalidos);
        bool tieneTextoAdicional = !esperada.PermitirTextoAdicional &&
            ContieneTextoAdicional(salida, candidatos.Select(candidato =>
                new IntervaloSecuencia(candidato.Indice, candidato.Longitud)), esperada);

        if (tieneTextoAdicional) {
            elementosAdicionales.Add("Texto adicional");
        }

        bool coincide =
            cantidadCorrecta &&
            ordenCorrecto &&
            faltantes.Count == 0 &&
            (esperada.PermitirElementosAdicionales ||
             elementosAdicionales.Count == 0) &&
            (esperada.PermitirDuplicados || duplicados.Count == 0) &&
            separadoresInvalidos.Count == 0 &&
            !tieneTextoAdicional;
        string mensaje = CrearMensajeSecuencia(
            coincide,
            cantidadCorrecta,
            ordenCorrecto,
            faltantes.Count,
            elementosAdicionales.Count,
            duplicados.Count,
            primerIndiceDiferente);

        return new ResultadoSecuenciaComparada {
            Nombre = esperada.Nombre,
            Tipo = esperada.Tipo,
            SecuenciaEncontrada = Array.AsReadOnly(
                encontrados.Select(FormatearNumeroSecuencia).ToArray()),
            CantidadEsperada = cantidadEsperada,
            CantidadEncontrada = encontrados.Length,
            CantidadCorrecta = cantidadCorrecta,
            OrdenCorrecto = ordenCorrecto,
            ElementosFaltantes = Array.AsReadOnly(
                faltantes.Select(FormatearNumeroSecuencia).ToArray()),
            ElementosAdicionales = elementosAdicionales.AsReadOnly(),
            DuplicadosInesperados = Array.AsReadOnly(
                duplicados.Select(FormatearNumeroSecuencia).ToArray()),
            PrimerIndiceDiferente = primerIndiceDiferente,
            Coincide = coincide,
            Mensaje = mensaje
        };
    }

    private static ResultadoSecuenciaComparada CompararSecuenciaTextual(
        string salida,
        SecuenciaEsperada esperada) {
        IReadOnlyList<CandidatoSecuenciaTextual> candidatos =
            ExtraerTextosSecuencia(salida, esperada);
        string[] encontrados = candidatos
            .Select(candidato => candidato.Valor)
            .ToArray();
        string[] valoresEsperados = esperada.AlternativasTextualesEsperadas
            .Select(elemento => elemento.Valor)
            .ToArray();
        int cantidadEsperada = esperada.CantidadExacta ?? valoresEsperados.Length;
        bool cantidadCorrecta = esperada.PermitirElementosAdicionales
            ? encontrados.Length >= cantidadEsperada
            : encontrados.Length == cantidadEsperada;
        bool ordenCorrecto = !esperada.OrdenObligatorio ||
            CoincideOrdenTextual(
                valoresEsperados,
                encontrados,
                esperada.PermitirElementosAdicionales);
        int? primerIndiceDiferente = esperada.OrdenObligatorio
            ? EncontrarPrimerIndiceDiferente(valoresEsperados, encontrados)
            : null;
        (IReadOnlyList<string> faltantes, IReadOnlyList<string> adicionales) =
            CalcularDiferenciasTextuales(valoresEsperados, encontrados);
        IReadOnlyList<string> duplicados = esperada.PermitirDuplicados
            ? Array.Empty<string>()
            : encontrados
                .GroupBy(valor => valor, StringComparer.OrdinalIgnoreCase)
                .Where(grupo => grupo.Count() > 1)
                .Select(grupo => grupo.Key)
                .ToArray();
        List<string> elementosAdicionales = adicionales.ToList();
        bool tieneTextoAdicional = !esperada.PermitirTextoAdicional &&
            ContieneTextoAdicional(salida, candidatos.Select(candidato =>
                new IntervaloSecuencia(candidato.Indice, candidato.Longitud)), esperada);

        if (tieneTextoAdicional) {
            elementosAdicionales.Add("Texto adicional");
        }

        bool coincide =
            cantidadCorrecta &&
            ordenCorrecto &&
            faltantes.Count == 0 &&
            (esperada.PermitirElementosAdicionales ||
             elementosAdicionales.Count == 0) &&
            (esperada.PermitirDuplicados || duplicados.Count == 0) &&
            !tieneTextoAdicional;
        string mensaje = CrearMensajeSecuencia(
            coincide,
            cantidadCorrecta,
            ordenCorrecto,
            faltantes.Count,
            elementosAdicionales.Count,
            duplicados.Count,
            primerIndiceDiferente);

        return new ResultadoSecuenciaComparada {
            Nombre = esperada.Nombre,
            Tipo = esperada.Tipo,
            SecuenciaEncontrada = Array.AsReadOnly(encontrados),
            CantidadEsperada = cantidadEsperada,
            CantidadEncontrada = encontrados.Length,
            CantidadCorrecta = cantidadCorrecta,
            OrdenCorrecto = ordenCorrecto,
            ElementosFaltantes = faltantes,
            ElementosAdicionales = elementosAdicionales.AsReadOnly(),
            DuplicadosInesperados = duplicados,
            PrimerIndiceDiferente = primerIndiceDiferente,
            Coincide = coincide,
            Mensaje = mensaje
        };
    }

    private static ResultadoSecuenciaComparada CrearResultadoSecuenciaInvalida(
        SecuenciaEsperada esperada,
        string mensaje) {
        return new ResultadoSecuenciaComparada {
            Nombre = esperada.Nombre,
            Tipo = esperada.Tipo,
            CantidadEsperada = esperada.CantidadExacta ?? 0,
            Mensaje = mensaje
        };
    }

    private static ResultadoSecuenciaCompuestaComparada CompararSecuenciaCompuesta(
        string salida,
        SecuenciaCompuestaEsperada esperada) {
        if (!IntentarCrearRegexPasoCompuesto(esperada, out Regex expresion)) {
            return CrearResultadoSecuenciaCompuestaInvalida(
                esperada,
                "La secuencia de filas no tiene una estructura válida.");
        }

        (
            IReadOnlyList<PasoCompuestoEncontrado> encontrados,
            IReadOnlyList<FilaCompuestaInvalida> invalidos) =
                ExtraerPasosCompuestos(salida, esperada, expresion);
        PasoSecuenciaCompuestaEsperado[] pasosEsperados =
            esperada.PasosEsperados.ToArray();
        int cantidadEsperada = esperada.CantidadExacta ?? pasosEsperados.Length;
        bool cantidadCorrecta = esperada.PermitirPasosAdicionales
            ? encontrados.Count >= cantidadEsperada
            : encontrados.Count == cantidadEsperada && invalidos.Count == 0;
        bool ordenCorrecto = !esperada.OrdenObligatorio ||
            CoincideOrdenPasosCompuestos(
                pasosEsperados,
                encontrados,
                esperada.PermitirPasosAdicionales);
        (
            IReadOnlyList<PasoSecuenciaCompuestaEsperado> faltantes,
            IReadOnlyList<PasoCompuestoEncontrado> adicionales) =
                CalcularDiferenciasPasosCompuestos(pasosEsperados, encontrados);
        IReadOnlyList<PasoCompuestoEncontrado> duplicados =
            esperada.PermitirPasosDuplicados
                ? Array.Empty<PasoCompuestoEncontrado>()
                : EncontrarDuplicadosPasosCompuestos(encontrados);
        List<string> filasAdicionales = adicionales
            .Select(paso => paso.Representacion)
            .Concat(invalidos.Select(fila => fila.Representacion))
            .ToList();
        List<ResultadoFilaSecuenciaCompuesta> filas = CrearResultadosFilasCompuestas(
            pasosEsperados,
            encontrados);
        int? primeraFilaIncorrecta = EncontrarPrimeraFilaCompuestaIncorrecta(
            filas,
            pasosEsperados.Length,
            encontrados.Count,
            invalidos);
        bool coincide =
            cantidadCorrecta &&
            ordenCorrecto &&
            faltantes.Count == 0 &&
            (esperada.PermitirPasosAdicionales || filasAdicionales.Count == 0) &&
            (esperada.PermitirPasosDuplicados || duplicados.Count == 0) &&
            invalidos.Count == 0;

        return new ResultadoSecuenciaCompuestaComparada {
            Nombre = esperada.Nombre,
            Filas = filas.AsReadOnly(),
            CantidadEsperada = cantidadEsperada,
            CantidadEncontrada = encontrados.Count,
            CantidadCorrecta = cantidadCorrecta,
            OrdenCorrecto = ordenCorrecto,
            FilasFaltantes = Array.AsReadOnly(
                faltantes.Select(FormatearPasoCompuesto).ToArray()),
            FilasDuplicadas = Array.AsReadOnly(
                duplicados.Select(paso => paso.Representacion).ToArray()),
            FilasAdicionales = filasAdicionales.AsReadOnly(),
            PrimeraFilaIncorrecta = primeraFilaIncorrecta,
            Coincide = coincide,
            Mensaje = CrearMensajeSecuenciaCompuesta(
                coincide,
                cantidadCorrecta,
                ordenCorrecto,
                filas,
                faltantes.Count,
                filasAdicionales.Count,
                duplicados.Count,
                invalidos.Count,
                primeraFilaIncorrecta)
        };
    }

    private static bool IntentarCrearRegexPasoCompuesto(
        SecuenciaCompuestaEsperada esperada,
        out Regex expresion) {
        expresion = RegexNumeroSecuenciaConComaDecimal();
        PasoSecuenciaCompuestaEsperado? primerPaso =
            esperada.PasosEsperados.FirstOrDefault();
        ComponenteNumericoPasoEsperado[] componentes = primerPaso?.Componentes
            .OrderBy(componente => componente.Posicion)
            .ToArray() ?? Array.Empty<ComponenteNumericoPasoEsperado>();

        if (!esperada.RequerirMismaLinea ||
            componentes.Length != 3 ||
            componentes.Select(componente => componente.Posicion)
                .Distinct()
                .Count() != 3 ||
            componentes.Skip(1)
                .SelectMany(componente =>
                    componente.EtiquetasOSeparadoresOpcionales)
                .Any(separador =>
                    !esperada.SeparadoresTextualesPermitidos.Contains(
                        separador,
                        StringComparer.OrdinalIgnoreCase))) {
            return false;
        }

        string numero =
            @"[-+]?(?:\d+(?:[.,]\d+)?|[.,]\d+)";
        string separadorSegundo = CrearPatronSeparadores(
            componentes[1].EtiquetasOSeparadoresOpcionales);
        string separadorTercero = CrearPatronSeparadores(
            componentes[2].EtiquetasOSeparadoresOpcionales);

        if (separadorSegundo.Length == 0 || separadorTercero.Length == 0) {
            return false;
        }

        expresion = new Regex(
            $@"(?<![\p{{L}}\p{{N}}_])(?<c0>{numero})\s*" +
            $@"(?:{separadorSegundo})\s*(?<c1>{numero})\s*" +
            $@"(?:{separadorTercero})\s*(?<c2>{numero})" +
            @"(?![\p{L}\p{N}_])",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        return true;
    }

    private static string CrearPatronSeparadores(
        IReadOnlyList<string> separadores) {
        return string.Join(
            "|",
            separadores
                .Where(separador => !string.IsNullOrWhiteSpace(separador))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(separador => separador.Length)
                .Select(Regex.Escape));
    }

    private static (
        IReadOnlyList<PasoCompuestoEncontrado> Pasos,
        IReadOnlyList<FilaCompuestaInvalida> Invalidos)
        ExtraerPasosCompuestos(
            string salida,
            SecuenciaCompuestaEsperada esperada,
            Regex expresion) {
        List<PasoCompuestoEncontrado> pasos = new();
        List<FilaCompuestaInvalida> invalidos = new();
        string[] lineas = SepararLineas(salida);

        for (int indiceLinea = 0; indiceLinea < lineas.Length; indiceLinea++) {
            string linea = lineas[indiceLinea];

            if (string.IsNullOrWhiteSpace(linea)) {
                continue;
            }

            Match[] operaciones = expresion.Matches(linea)
                .Cast<Match>()
                .ToArray();
            Match[] numeros = RegexNumeroSecuenciaConComaDecimal()
                .Matches(linea)
                .Cast<Match>()
                .ToArray();

            if (operaciones.Length == 0 && numeros.Length == 0) {
                if (!esperada.PermitirTextoAdicional) {
                    invalidos.Add(new FilaCompuestaInvalida(
                        indiceLinea + 1,
                        linea.Trim()));
                }

                continue;
            }

            if (operaciones.Length != 1 || numeros.Length != 3) {
                invalidos.Add(new FilaCompuestaInvalida(
                    indiceLinea + 1,
                    linea.Trim()));
                continue;
            }

            Match operacion = operaciones[0];

            if (!IntentarConvertirNumero(operacion.Groups["c0"].Value, out double primero) ||
                !IntentarConvertirNumero(operacion.Groups["c1"].Value, out double segundo) ||
                !IntentarConvertirNumero(operacion.Groups["c2"].Value, out double tercero)) {
                invalidos.Add(new FilaCompuestaInvalida(
                    indiceLinea + 1,
                    linea.Trim()));
                continue;
            }

            pasos.Add(new PasoCompuestoEncontrado(
                indiceLinea + 1,
                linea.Trim(),
                Array.AsReadOnly(new[] { primero, segundo, tercero })));
        }

        return (pasos, invalidos);
    }

    private static bool CoincideOrdenPasosCompuestos(
        IReadOnlyList<PasoSecuenciaCompuestaEsperado> esperados,
        IReadOnlyList<PasoCompuestoEncontrado> encontrados,
        bool permitirAdicionales) {
        if (!permitirAdicionales) {
            return esperados.Count == encontrados.Count &&
                esperados.Select((paso, indice) =>
                    CoincidePasoCompuesto(paso, encontrados[indice]))
                .All(coincide => coincide);
        }

        int indiceEsperado = 0;

        foreach (PasoCompuestoEncontrado encontrado in encontrados) {
            if (indiceEsperado < esperados.Count &&
                CoincidePasoCompuesto(esperados[indiceEsperado], encontrado)) {
                indiceEsperado++;
            }
        }

        return indiceEsperado == esperados.Count;
    }

    private static (
        IReadOnlyList<PasoSecuenciaCompuestaEsperado> Faltantes,
        IReadOnlyList<PasoCompuestoEncontrado> Adicionales)
        CalcularDiferenciasPasosCompuestos(
            IReadOnlyList<PasoSecuenciaCompuestaEsperado> esperados,
            IReadOnlyList<PasoCompuestoEncontrado> encontrados) {
        bool[] utilizados = new bool[encontrados.Count];
        List<PasoSecuenciaCompuestaEsperado> faltantes = new();

        foreach (PasoSecuenciaCompuestaEsperado esperado in esperados) {
            int indiceEncontrado = -1;

            for (int indice = 0; indice < encontrados.Count; indice++) {
                if (!utilizados[indice] &&
                    CoincidePasoCompuesto(esperado, encontrados[indice])) {
                    indiceEncontrado = indice;
                    break;
                }
            }

            if (indiceEncontrado < 0) {
                faltantes.Add(esperado);
            } else {
                utilizados[indiceEncontrado] = true;
            }
        }

        PasoCompuestoEncontrado[] adicionales = encontrados
            .Where((_, indice) => !utilizados[indice])
            .ToArray();
        return (faltantes, adicionales);
    }

    private static IReadOnlyList<PasoCompuestoEncontrado>
        EncontrarDuplicadosPasosCompuestos(
            IReadOnlyList<PasoCompuestoEncontrado> encontrados) {
        List<PasoCompuestoEncontrado> revisados = new();
        List<PasoCompuestoEncontrado> duplicados = new();

        foreach (PasoCompuestoEncontrado encontrado in encontrados) {
            bool repetido = revisados.Any(revisado =>
                SonPasosEncontradosEquivalentes(revisado, encontrado));

            if (repetido &&
                !duplicados.Any(duplicado =>
                    SonPasosEncontradosEquivalentes(duplicado, encontrado))) {
                duplicados.Add(encontrado);
            } else if (!repetido) {
                revisados.Add(encontrado);
            }
        }

        return duplicados;
    }

    private static bool CoincidePasoCompuesto(
        PasoSecuenciaCompuestaEsperado esperado,
        PasoCompuestoEncontrado encontrado) {
        ComponenteNumericoPasoEsperado[] componentes = esperado.Componentes
            .OrderBy(componente => componente.Posicion)
            .ToArray();

        return componentes.Length == encontrado.Valores.Count &&
            componentes.Select((componente, indice) =>
                SonEquivalentes(
                    componente.Valor,
                    encontrado.Valores[indice],
                    componente.Tolerancia))
            .All(coincide => coincide);
    }

    private static bool SonPasosEncontradosEquivalentes(
        PasoCompuestoEncontrado primero,
        PasoCompuestoEncontrado segundo) {
        const double tolerancia = 0.000_001D;
        return primero.Valores.Count == segundo.Valores.Count &&
            primero.Valores.Select((valor, indice) =>
                SonEquivalentes(
                    valor,
                    segundo.Valores[indice],
                    tolerancia))
            .All(coincide => coincide);
    }

    private static List<ResultadoFilaSecuenciaCompuesta>
        CrearResultadosFilasCompuestas(
            IReadOnlyList<PasoSecuenciaCompuestaEsperado> esperados,
            IReadOnlyList<PasoCompuestoEncontrado> encontrados) {
        List<ResultadoFilaSecuenciaCompuesta> filas = new();
        int cantidad = Math.Max(esperados.Count, encontrados.Count);

        for (int indice = 0; indice < cantidad; indice++) {
            PasoSecuenciaCompuestaEsperado? esperado =
                indice < esperados.Count ? esperados[indice] : null;
            PasoCompuestoEncontrado? encontrado =
                indice < encontrados.Count ? encontrados[indice] : null;
            ComponenteNumericoPasoEsperado[] componentes = esperado?.Componentes
                .OrderBy(componente => componente.Posicion)
                .ToArray() ?? Array.Empty<ComponenteNumericoPasoEsperado>();

            filas.Add(new ResultadoFilaSecuenciaCompuesta {
                NumeroFila = indice + 1,
                FilaEsperada = esperado is null
                    ? string.Empty
                    : FormatearPasoCompuesto(esperado),
                FilaEncontrada = encontrado?.Representacion ?? string.Empty,
                BaseCorrecta = CoincideComponente(componentes, encontrado, 0),
                MultiplicadorCorrecto = CoincideComponente(componentes, encontrado, 1),
                ResultadoCorrecto = CoincideComponente(componentes, encontrado, 2)
            });
        }

        return filas;
    }

    private static bool CoincideComponente(
        IReadOnlyList<ComponenteNumericoPasoEsperado> componentes,
        PasoCompuestoEncontrado? encontrado,
        int posicion) {
        return encontrado is not null &&
            posicion < componentes.Count &&
            posicion < encontrado.Valores.Count &&
            SonEquivalentes(
                componentes[posicion].Valor,
                encontrado.Valores[posicion],
                componentes[posicion].Tolerancia);
    }

    private static int? EncontrarPrimeraFilaCompuestaIncorrecta(
        IReadOnlyList<ResultadoFilaSecuenciaCompuesta> filas,
        int cantidadEsperada,
        int cantidadEncontrada,
        IReadOnlyList<FilaCompuestaInvalida> invalidos) {
        int? filaComparada = filas
            .FirstOrDefault(fila => !fila.Coincide)
            ?.NumeroFila;
        int? filaInvalida = invalidos
            .OrderBy(fila => fila.NumeroLinea)
            .FirstOrDefault()
            ?.NumeroLinea;

        if (filaComparada.HasValue && filaInvalida.HasValue) {
            return Math.Min(filaComparada.Value, filaInvalida.Value);
        }

        if (filaComparada.HasValue) {
            return filaComparada;
        }

        if (filaInvalida.HasValue) {
            return filaInvalida;
        }

        return cantidadEsperada == cantidadEncontrada
            ? null
            : Math.Min(cantidadEsperada, cantidadEncontrada) + 1;
    }

    private static string FormatearPasoCompuesto(
        PasoSecuenciaCompuestaEsperado paso) {
        ComponenteNumericoPasoEsperado[] componentes = paso.Componentes
            .OrderBy(componente => componente.Posicion)
            .ToArray();

        return componentes.Length == 3
            ? $"{FormatearNumeroSecuencia(componentes[0].Valor)} x " +
              $"{FormatearNumeroSecuencia(componentes[1].Valor)} = " +
              FormatearNumeroSecuencia(componentes[2].Valor)
            : paso.Nombre;
    }

    private static string CrearMensajeSecuenciaCompuesta(
        bool coincide,
        bool cantidadCorrecta,
        bool ordenCorrecto,
        IReadOnlyList<ResultadoFilaSecuenciaCompuesta> filas,
        int faltantes,
        int adicionales,
        int duplicados,
        int invalidos,
        int? primeraFilaIncorrecta) {
        if (coincide) {
            return "La tabla contiene las diez operaciones correctas y en orden.";
        }

        if (invalidos > 0) {
            return "Cada operación debe aparecer como una fila independiente con base, multiplicador y resultado.";
        }

        if (!cantidadCorrecta) {
            return "La tabla debe contener exactamente diez filas.";
        }

        if (duplicados > 0) {
            return "La tabla contiene una fila duplicada.";
        }

        if (!ordenCorrecto && faltantes == 0 && adicionales == 0) {
            return "Revisa el orden de los multiplicadores de la tabla.";
        }

        ResultadoFilaSecuenciaCompuesta? filaIncorrecta = filas
            .FirstOrDefault(fila => !fila.Coincide);

        if (filaIncorrecta is not null) {
            string numero = primeraFilaIncorrecta.HasValue
                ? $" {primeraFilaIncorrecta.Value}"
                : string.Empty;

            if (!filaIncorrecta.BaseCorrecta) {
                return $"Revisa el número base de la fila{numero}.";
            }

            if (!filaIncorrecta.MultiplicadorCorrecto) {
                return $"Revisa el multiplicador de la fila{numero}.";
            }

            if (!filaIncorrecta.ResultadoCorrecto) {
                return $"Revisa el resultado de la fila{numero}.";
            }
        }

        if (faltantes > 0) {
            return "Falta una o más filas de la tabla.";
        }

        return adicionales > 0
            ? "La tabla contiene una o más filas adicionales."
            : "Revisa la estructura de la tabla.";
    }

    private static ResultadoSecuenciaCompuestaComparada
        CrearResultadoSecuenciaCompuestaInvalida(
            SecuenciaCompuestaEsperada esperada,
            string mensaje) {
        return new ResultadoSecuenciaCompuestaComparada {
            Nombre = esperada.Nombre,
            CantidadEsperada = esperada.CantidadExacta ?? 0,
            Mensaje = mensaje
        };
    }

    private static IReadOnlyList<CandidatoSecuenciaNumerica> ExtraerNumerosSecuencia(
        string salida,
        SecuenciaEsperada esperada) {
        bool comaEsSeparador = esperada.SeparadoresPermitidos.Contains(
            ",",
            StringComparer.Ordinal);
        Regex expresion = comaEsSeparador
            ? RegexNumeroSecuenciaConComaSeparador()
            : RegexNumeroSecuenciaConComaDecimal();
        List<CandidatoSecuenciaNumerica> candidatos = new();

        foreach (Match coincidencia in expresion.Matches(salida).Cast<Match>()) {
            if (IntentarConvertirNumero(coincidencia.Value, out double valor)) {
                candidatos.Add(new CandidatoSecuenciaNumerica(
                    coincidencia.Index,
                    coincidencia.Length,
                    valor));
            }
        }

        return candidatos;
    }

    private static IReadOnlyList<CandidatoSecuenciaTextual> ExtraerTextosSecuencia(
        string salida,
        SecuenciaEsperada esperada) {
        string salidaNormalizada = NormalizarLineaPreservandoIndices(salida);
        List<CandidatoSecuenciaTextual> candidatos = new();

        foreach (ElementoTextualSecuenciaEsperado elemento in
            esperada.AlternativasTextualesEsperadas) {
            IEnumerable<string> representaciones = new[] { elemento.Valor }
                .Concat(elemento.Alternativas)
                .Where(representacion => !string.IsNullOrWhiteSpace(representacion))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(representacion => representacion.Length);

            foreach (string representacion in representaciones) {
                string normalizada = NormalizarTexto(representacion);

                if (normalizada.Length == 0) {
                    continue;
                }

                foreach (Match coincidencia in CrearRegexToken(normalizada)
                    .Matches(salidaNormalizada)
                    .Cast<Match>()) {
                    candidatos.Add(new CandidatoSecuenciaTextual(
                        coincidencia.Index,
                        coincidencia.Length,
                        elemento.Valor));
                }
            }
        }

        List<CandidatoSecuenciaTextual> seleccionados = new();

        foreach (CandidatoSecuenciaTextual candidato in candidatos
            .OrderBy(candidato => candidato.Indice)
            .ThenByDescending(candidato => candidato.Longitud)) {
            bool seSuperpone = seleccionados.Any(seleccionado =>
                candidato.Indice < seleccionado.Indice + seleccionado.Longitud &&
                seleccionado.Indice < candidato.Indice + candidato.Longitud);

            if (!seSuperpone) {
                seleccionados.Add(candidato);
            }
        }

        return seleccionados;
    }

    private static bool CoincideOrdenNumerico(
        IReadOnlyList<double> esperados,
        IReadOnlyList<double> encontrados,
        double tolerancia,
        bool permitirAdicionales) {
        if (!permitirAdicionales) {
            return esperados.Count == encontrados.Count &&
                esperados.Select((valor, indice) =>
                    SonEquivalentes(valor, encontrados[indice], tolerancia))
                .All(coincide => coincide);
        }

        int indiceEsperado = 0;

        foreach (double encontrado in encontrados) {
            if (indiceEsperado < esperados.Count &&
                SonEquivalentes(
                    esperados[indiceEsperado],
                    encontrado,
                    tolerancia)) {
                indiceEsperado++;
            }
        }

        return indiceEsperado == esperados.Count;
    }

    private static bool CoincideOrdenTextual(
        IReadOnlyList<string> esperados,
        IReadOnlyList<string> encontrados,
        bool permitirAdicionales) {
        if (!permitirAdicionales) {
            return esperados.Count == encontrados.Count &&
                esperados.Select((valor, indice) =>
                    valor.Equals(
                        encontrados[indice],
                        StringComparison.OrdinalIgnoreCase))
                .All(coincide => coincide);
        }

        int indiceEsperado = 0;

        foreach (string encontrado in encontrados) {
            if (indiceEsperado < esperados.Count &&
                esperados[indiceEsperado].Equals(
                    encontrado,
                    StringComparison.OrdinalIgnoreCase)) {
                indiceEsperado++;
            }
        }

        return indiceEsperado == esperados.Count;
    }

    private static int? EncontrarPrimerIndiceDiferente(
        IReadOnlyList<double> esperados,
        IReadOnlyList<double> encontrados,
        double tolerancia) {
        int cantidadComun = Math.Min(esperados.Count, encontrados.Count);

        for (int indice = 0; indice < cantidadComun; indice++) {
            if (!SonEquivalentes(
                esperados[indice],
                encontrados[indice],
                tolerancia)) {
                return indice;
            }
        }

        return esperados.Count == encontrados.Count
            ? null
            : cantidadComun;
    }

    private static int? EncontrarPrimerIndiceDiferente(
        IReadOnlyList<string> esperados,
        IReadOnlyList<string> encontrados) {
        int cantidadComun = Math.Min(esperados.Count, encontrados.Count);

        for (int indice = 0; indice < cantidadComun; indice++) {
            if (!esperados[indice].Equals(
                encontrados[indice],
                StringComparison.OrdinalIgnoreCase)) {
                return indice;
            }
        }

        return esperados.Count == encontrados.Count
            ? null
            : cantidadComun;
    }

    private static (
        IReadOnlyList<double> Faltantes,
        IReadOnlyList<double> Adicionales)
        CalcularDiferenciasNumericas(
            IReadOnlyList<double> esperados,
            IReadOnlyList<double> encontrados,
            double tolerancia) {
        bool[] utilizados = new bool[encontrados.Count];
        List<double> faltantes = new();

        foreach (double esperado in esperados) {
            int indice = -1;

            for (int candidato = 0; candidato < encontrados.Count; candidato++) {
                if (!utilizados[candidato] &&
                    SonEquivalentes(
                        esperado,
                        encontrados[candidato],
                        tolerancia)) {
                    indice = candidato;
                    break;
                }
            }

            if (indice < 0) {
                faltantes.Add(esperado);
            } else {
                utilizados[indice] = true;
            }
        }

        double[] adicionales = encontrados
            .Where((_, indice) => !utilizados[indice])
            .ToArray();
        return (faltantes, adicionales);
    }

    private static (
        IReadOnlyList<string> Faltantes,
        IReadOnlyList<string> Adicionales)
        CalcularDiferenciasTextuales(
            IReadOnlyList<string> esperados,
            IReadOnlyList<string> encontrados) {
        bool[] utilizados = new bool[encontrados.Count];
        List<string> faltantes = new();

        foreach (string esperado in esperados) {
            int indice = -1;

            for (int candidato = 0; candidato < encontrados.Count; candidato++) {
                if (!utilizados[candidato] &&
                    esperado.Equals(
                        encontrados[candidato],
                        StringComparison.OrdinalIgnoreCase)) {
                    indice = candidato;
                    break;
                }
            }

            if (indice < 0) {
                faltantes.Add(esperado);
            } else {
                utilizados[indice] = true;
            }
        }

        string[] adicionales = encontrados
            .Where((_, indice) => !utilizados[indice])
            .ToArray();
        return (faltantes, adicionales);
    }

    private static IReadOnlyList<double> EncontrarDuplicadosNumericos(
        IReadOnlyList<double> valores,
        double tolerancia) {
        List<double> revisados = new();
        List<double> duplicados = new();

        foreach (double valor in valores) {
            bool yaExistia = revisados.Any(revisado =>
                SonEquivalentes(revisado, valor, tolerancia));

            if (yaExistia) {
                if (!duplicados.Any(duplicado =>
                    SonEquivalentes(duplicado, valor, tolerancia))) {
                    duplicados.Add(valor);
                }
            } else {
                revisados.Add(valor);
            }
        }

        return duplicados;
    }

    private static IReadOnlyList<string> EncontrarSeparadoresNumericosInvalidos(
        string salida,
        IReadOnlyList<CandidatoSecuenciaNumerica> candidatos,
        SecuenciaEsperada esperada) {
        List<string> invalidos = new();

        for (int indice = 1; indice < candidatos.Count; indice++) {
            CandidatoSecuenciaNumerica anterior = candidatos[indice - 1];
            CandidatoSecuenciaNumerica actual = candidatos[indice];
            int inicio = anterior.Indice + anterior.Longitud;
            int longitud = Math.Max(0, actual.Indice - inicio);
            string segmento = salida.Substring(inicio, longitud);

            if (segmento.Any(char.IsLetterOrDigit) &&
                esperada.PermitirTextoAdicional) {
                continue;
            }

            if (!EsCombinacionDeSeparadoresPermitidos(
                segmento,
                esperada.SeparadoresPermitidos)) {
                string representacion = segmento.Length == 0
                    ? "sin separación"
                    : segmento.Replace("\r", "\\r", StringComparison.Ordinal)
                        .Replace("\n", "\\n", StringComparison.Ordinal)
                        .Replace("\t", "\\t", StringComparison.Ordinal);
                invalidos.Add($"Separador no permitido: {representacion}");
            }
        }

        return invalidos
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static bool EsCombinacionDeSeparadoresPermitidos(
        string segmento,
        IReadOnlyList<string> separadoresPermitidos) {
        if (segmento.Length == 0 || separadoresPermitidos.Count == 0) {
            return false;
        }

        string[] separadores = separadoresPermitidos
            .Where(separador => !string.IsNullOrEmpty(separador))
            .Distinct(StringComparer.Ordinal)
            .OrderByDescending(separador => separador.Length)
            .ToArray();
        int indice = 0;

        while (indice < segmento.Length) {
            string? separador = separadores.FirstOrDefault(elemento =>
                segmento.AsSpan(indice).StartsWith(
                    elemento,
                    StringComparison.Ordinal));

            if (separador is null) {
                return false;
            }

            indice += separador.Length;
        }

        return true;
    }

    private static bool ContieneTextoAdicional(
        string salida,
        IEnumerable<IntervaloSecuencia> intervalos,
        SecuenciaEsperada esperada) {
        bool[] consumidos = new bool[salida.Length];

        foreach (IntervaloSecuencia intervalo in intervalos) {
            int final = Math.Min(
                salida.Length,
                intervalo.Indice + intervalo.Longitud);

            for (int indice = Math.Max(0, intervalo.Indice); indice < final; indice++) {
                consumidos[indice] = true;
            }
        }

        int posicion = 0;

        while (posicion < salida.Length) {
            if (consumidos[posicion]) {
                posicion++;
                continue;
            }

            string? separador = esperada.SeparadoresPermitidos
                .Where(elemento => !string.IsNullOrEmpty(elemento))
                .OrderByDescending(elemento => elemento.Length)
                .FirstOrDefault(elemento =>
                    salida.AsSpan(posicion).StartsWith(
                        elemento,
                        StringComparison.Ordinal));

            if (separador is not null) {
                posicion += separador.Length;
                continue;
            }

            return true;
        }

        return false;
    }

    private static string FormatearNumeroSecuencia(double valor) {
        return valor.ToString("0.################", CultureInfo.InvariantCulture);
    }

    private static string CrearMensajeSecuencia(
        bool coincide,
        bool cantidadCorrecta,
        bool ordenCorrecto,
        int faltantes,
        int adicionales,
        int duplicados,
        int? primerIndiceDiferente) {
        if (coincide) {
            return "La secuencia tiene los valores, el orden y la cantidad esperados.";
        }

        if (!cantidadCorrecta) {
            return "Revisa cuántos elementos muestra la secuencia.";
        }

        if (!ordenCorrecto) {
            return primerIndiceDiferente.HasValue
                ? $"Revisa el orden desde el elemento {primerIndiceDiferente.Value + 1}."
                : "Revisa el orden de los elementos.";
        }

        if (duplicados > 0) {
            return "La secuencia contiene elementos repetidos que no se esperaban.";
        }

        if (faltantes > 0) {
            return "La secuencia no contiene todos los elementos esperados.";
        }

        return adicionales > 0
            ? "La secuencia contiene elementos o separadores adicionales."
            : "Revisa los elementos de la secuencia.";
    }

    private static ResultadoValorNumericoComparado CompararValorNumerico(
        string salida,
        ValorNumericoEsperado esperado,
        HashSet<PosicionNumero> numerosUtilizados) {
        string[] lineas = SepararLineas(salida);
        IReadOnlyList<string> etiquetas = CrearEtiquetasBusqueda(esperado);
        Dictionary<PosicionNumero, CandidatoNumero> candidatos = new();

        foreach (string etiqueta in etiquetas) {
            string etiquetaNormalizada = NormalizarTexto(etiqueta);

            if (string.IsNullOrWhiteSpace(etiquetaNormalizada)) {
                continue;
            }

            for (int indiceLinea = 0; indiceLinea < lineas.Length; indiceLinea++) {
                string lineaNormalizada = NormalizarLineaPreservandoIndices(
                    lineas[indiceLinea]);

                foreach (Match coincidenciaEtiqueta in CrearRegexToken(etiquetaNormalizada)
                    .Matches(lineaNormalizada)
                    .Cast<Match>()) {
                    foreach (CandidatoNumero candidato in ObtenerNumerosAsociados(
                        lineas,
                        indiceLinea,
                        coincidenciaEtiqueta,
                        etiqueta)) {
                        PosicionNumero posicion = new(
                            candidato.IndiceLinea,
                            candidato.IndiceCaracter,
                            candidato.Longitud);

                        if (numerosUtilizados.Contains(posicion)) {
                            continue;
                        }

                        candidatos.TryAdd(posicion, candidato);
                    }
                }
            }
        }

        foreach (PosicionNumero posicion in candidatos.Keys) {
            numerosUtilizados.Add(posicion);
        }

        CandidatoNumero[] encontrados = candidatos.Values.ToArray();
        bool tieneContradiccion = encontrados.Any(candidato =>
            encontrados.Any(otro => !SonRepresentacionesNumericasEquivalentes(
                candidato.Valor,
                otro.Valor,
                esperado)));
        bool coincide = esperado.DebeEstarAusente
            ? encontrados.Length == 0
            : encontrados.Length == 0
                ? esperado.EsOpcional
                : !tieneContradiccion &&
                    encontrados.All(candidato => CoincideValorNumericoEsperado(
                        candidato.Valor,
                        esperado));
        CandidatoNumero? primerCandidato = encontrados.FirstOrDefault();

        return CrearResultadoValor(
            esperado,
            primerCandidato?.Valor,
            coincide,
            primerCandidato?.Etiqueta ?? string.Empty,
            tieneContradiccion,
            encontrados.Select(candidato => candidato.Valor).ToArray());
    }

    private static ResultadoValorBooleanoComparado CompararValorBooleano(
        string salida,
        ValorBooleanoEsperado esperado,
        HashSet<PosicionNumero> booleanosUtilizados) {
        string[] lineas = SepararLineas(salida);
        IReadOnlyList<string> etiquetas = CrearEtiquetasBusqueda(esperado);
        Dictionary<PosicionNumero, CandidatoBooleano> candidatos = new();

        foreach (string etiqueta in etiquetas) {
            string etiquetaNormalizada = NormalizarTexto(etiqueta);

            if (string.IsNullOrWhiteSpace(etiquetaNormalizada)) {
                continue;
            }

            for (int indiceLinea = 0; indiceLinea < lineas.Length; indiceLinea++) {
                string lineaNormalizada = NormalizarLineaPreservandoIndices(
                    lineas[indiceLinea]);

                foreach (Match coincidenciaEtiqueta in CrearRegexToken(etiquetaNormalizada)
                    .Matches(lineaNormalizada)
                    .Cast<Match>()) {
                    CandidatoBooleano? candidato = ObtenerBooleanoAsociado(
                        lineas,
                        indiceLinea,
                        coincidenciaEtiqueta,
                        etiqueta,
                        esperado);

                    if (candidato is null) {
                        continue;
                    }

                    PosicionNumero posicion = new(
                        candidato.IndiceLinea,
                        candidato.IndiceCaracter,
                        candidato.Longitud);

                    if (!booleanosUtilizados.Contains(posicion)) {
                        candidatos.TryAdd(posicion, candidato);
                    }
                }
            }
        }

        foreach (PosicionNumero posicion in candidatos.Keys) {
            booleanosUtilizados.Add(posicion);
        }

        CandidatoBooleano[] encontrados = candidatos.Values.ToArray();
        bool tieneContradiccion = encontrados
            .Select(candidato => candidato.Valor)
            .Distinct()
            .Count() > 1;
        bool coincide = encontrados.Length > 0 &&
            !tieneContradiccion &&
            encontrados.All(candidato => candidato.Valor == esperado.Valor);
        CandidatoBooleano? primerCandidato = encontrados.FirstOrDefault();

        return new ResultadoValorBooleanoComparado {
            Nombre = esperado.Nombre,
            ValorEsperado = esperado.Valor,
            ValorObtenido = primerCandidato?.Valor,
            Coincide = coincide,
            EtiquetaEncontrada = primerCandidato?.Etiqueta ?? string.Empty,
            RepresentacionEncontrada = primerCandidato?.Representacion ?? string.Empty,
            TieneContradiccion = tieneContradiccion,
            UsoEtiquetaAlternativa = primerCandidato is not null &&
                EsEtiquetaAlternativa(primerCandidato.Etiqueta, esperado.EtiquetasAlternativas),
            ValoresEncontrados = Array.AsReadOnly(
                encontrados.Select(candidato => candidato.Valor).ToArray())
        };
    }

    private static ResultadoValorTextualComparado CompararValorTextual(
        string salida,
        ValorTextualEsperado esperado,
        HashSet<PosicionNumero> textosUtilizados) {
        string[] lineas = SepararLineas(salida);
        IReadOnlyList<string> etiquetas = CrearEtiquetasBusqueda(esperado);
        Dictionary<PosicionNumero, CandidatoTexto> candidatos = new();

        foreach (string etiqueta in etiquetas) {
            string etiquetaNormalizada = NormalizarTexto(etiqueta);

            if (string.IsNullOrWhiteSpace(etiquetaNormalizada)) {
                continue;
            }

            for (int indiceLinea = 0; indiceLinea < lineas.Length; indiceLinea++) {
                string lineaNormalizada = NormalizarLineaPreservandoIndices(
                    lineas[indiceLinea]);

                foreach (Match coincidenciaEtiqueta in CrearRegexToken(etiquetaNormalizada)
                    .Matches(lineaNormalizada)
                    .Cast<Match>()) {
                    CandidatoTexto? candidato = ObtenerTextoAsociado(
                        lineas,
                        indiceLinea,
                        coincidenciaEtiqueta,
                        etiqueta,
                        esperado);

                    if (candidato is null) {
                        continue;
                    }

                    PosicionNumero posicion = new(
                        candidato.IndiceLinea,
                        candidato.IndiceCaracter,
                        candidato.Longitud);

                    if (!textosUtilizados.Contains(posicion)) {
                        candidatos.TryAdd(posicion, candidato);
                    }
                }
            }
        }

        foreach (PosicionNumero posicion in candidatos.Keys) {
            textosUtilizados.Add(posicion);
        }

        CandidatoTexto[] encontrados = candidatos.Values.ToArray();
        bool tieneContradiccion = encontrados
            .Select(candidato => candidato.Valor)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count() > 1;
        bool coincide = encontrados.Length == 0
            ? esperado.EsOpcional
            : !tieneContradiccion &&
                encontrados.All(candidato => candidato.Valor.Equals(
                    esperado.Valor,
                    StringComparison.OrdinalIgnoreCase));
        CandidatoTexto? primerCandidato = encontrados.FirstOrDefault();

        return new ResultadoValorTextualComparado {
            Nombre = esperado.Nombre,
            ValorEsperado = esperado.Valor,
            EsOpcional = esperado.EsOpcional,
            ValorObtenido = primerCandidato?.Valor ?? string.Empty,
            Coincide = coincide,
            EtiquetaEncontrada = primerCandidato?.Etiqueta ?? string.Empty,
            RepresentacionEncontrada = primerCandidato?.Representacion ?? string.Empty,
            TieneContradiccion = tieneContradiccion,
            UsoEtiquetaAlternativa = primerCandidato is not null &&
                EsEtiquetaAlternativa(primerCandidato.Etiqueta, esperado.EtiquetasAlternativas),
            ValoresEncontrados = Array.AsReadOnly(
                encontrados.Select(candidato => candidato.Valor).ToArray())
        };
    }

    private static IEnumerable<CandidatoNumero> ObtenerNumerosAsociados(
        string[] lineas,
        int indiceLinea,
        Match coincidenciaEtiqueta,
        string etiqueta) {
        string linea = lineas[indiceLinea];
        int inicioBusqueda = Math.Min(coincidenciaEtiqueta.Index + coincidenciaEtiqueta.Length, linea.Length);
        string segmento = linea[inicioBusqueda..];
        int espaciosIniciales = segmento.Length - segmento.TrimStart().Length;
        segmento = segmento[espaciosIniciales..];
        inicioBusqueda += espaciosIniciales;

        if (segmento.StartsWith(':') || segmento.StartsWith('=')) {
            segmento = segmento[1..];
            inicioBusqueda++;
        }

        Match numero = RegexNumeroInicial().Match(segmento);
        if (numero.Success &&
            IntentarConvertirNumero(numero.Value, out double valorMismaLinea)) {
            yield return new CandidatoNumero(
                indiceLinea,
                inicioBusqueda + numero.Index,
                numero.Length,
                valorMismaLinea,
                etiqueta);
            yield break;
        }

        for (int siguiente = indiceLinea + 1; siguiente < lineas.Length; siguiente++) {
            string siguienteLinea = lineas[siguiente].Trim();

            if (siguienteLinea.Length == 0) {
                continue;
            }

            Match numeroInicial = RegexNumeroInicial().Match(siguienteLinea);

            if (numeroInicial.Success &&
                IntentarConvertirNumero(numeroInicial.Value, out double valor)) {
                yield return new CandidatoNumero(
                    siguiente,
                    numeroInicial.Index,
                    numeroInicial.Length,
                    valor,
                    etiqueta);
            }

            yield break;
        }
    }

    private static CandidatoBooleano? ObtenerBooleanoAsociado(
        string[] lineas,
        int indiceLinea,
        Match coincidenciaEtiqueta,
        string etiqueta,
        ValorBooleanoEsperado esperado) {
        string linea = lineas[indiceLinea];
        int inicioBusqueda = Math.Min(
            coincidenciaEtiqueta.Index + coincidenciaEtiqueta.Length,
            linea.Length);
        CandidatoBooleano? candidato = BuscarBooleanoAlInicio(
            linea[inicioBusqueda..],
            indiceLinea,
            inicioBusqueda,
            etiqueta,
            esperado);

        if (candidato is not null) {
            return candidato;
        }

        for (int siguiente = indiceLinea + 1; siguiente < lineas.Length; siguiente++) {
            if (string.IsNullOrWhiteSpace(lineas[siguiente])) {
                continue;
            }

            return BuscarBooleanoAlInicio(
                lineas[siguiente],
                siguiente,
                0,
                etiqueta,
                esperado);
        }

        return null;
    }

    private static CandidatoBooleano? BuscarBooleanoAlInicio(
        string segmento,
        int indiceLinea,
        int desplazamiento,
        string etiqueta,
        ValorBooleanoEsperado esperado) {
        int caracteresIgnorados = 0;

        while (caracteresIgnorados < segmento.Length &&
            (char.IsWhiteSpace(segmento[caracteresIgnorados]) ||
             segmento[caracteresIgnorados] is ':' or '=' or '-' or '–' or '—')) {
            caracteresIgnorados++;
        }

        string valorOriginal = segmento[caracteresIgnorados..];
        string valorNormalizado = NormalizarTexto(valorOriginal);

        foreach ((string representacion, bool valor) in CrearRepresentacionesBooleanas(esperado)) {
            string representacionNormalizada = NormalizarTexto(representacion);

            if (representacionNormalizada.Length == 0 ||
                !EmpiezaConToken(valorNormalizado, representacionNormalizada)) {
                continue;
            }

            return new CandidatoBooleano(
                indiceLinea,
                desplazamiento + caracteresIgnorados,
                representacion.Length,
                valor,
                etiqueta,
                representacion);
        }

        return null;
    }

    private static IReadOnlyList<(string Representacion, bool Valor)>
        CrearRepresentacionesBooleanas(ValorBooleanoEsperado esperado) {
        Dictionary<string, (string Representacion, bool Valor)> representaciones =
            new(StringComparer.Ordinal);

        foreach (string representacion in esperado.RepresentacionesVerdaderas) {
            string normalizada = NormalizarTexto(representacion);

            if (normalizada.Length > 0) {
                representaciones.TryAdd(normalizada, (representacion, true));
            }
        }

        foreach (string representacion in esperado.RepresentacionesFalsas) {
            string normalizada = NormalizarTexto(representacion);

            if (normalizada.Length > 0) {
                representaciones.TryAdd(normalizada, (representacion, false));
            }
        }

        return representaciones.Values
            .OrderByDescending(elemento => elemento.Representacion.Length)
            .ToArray();
    }

    private static CandidatoTexto? ObtenerTextoAsociado(
        string[] lineas,
        int indiceLinea,
        Match coincidenciaEtiqueta,
        string etiqueta,
        ValorTextualEsperado esperado) {
        string linea = lineas[indiceLinea];
        int inicioBusqueda = Math.Min(
            coincidenciaEtiqueta.Index + coincidenciaEtiqueta.Length,
            linea.Length);
        CandidatoTexto? candidato = BuscarTextoAlInicio(
            linea[inicioBusqueda..],
            indiceLinea,
            inicioBusqueda,
            etiqueta,
            esperado);

        if (candidato is not null) {
            return candidato;
        }

        for (int siguiente = indiceLinea + 1; siguiente < lineas.Length; siguiente++) {
            if (string.IsNullOrWhiteSpace(lineas[siguiente])) {
                continue;
            }

            return BuscarTextoAlInicio(
                lineas[siguiente],
                siguiente,
                0,
                etiqueta,
                esperado);
        }

        return null;
    }

    private static CandidatoTexto? BuscarTextoAlInicio(
        string segmento,
        int indiceLinea,
        int desplazamiento,
        string etiqueta,
        ValorTextualEsperado esperado) {
        int caracteresIgnorados = 0;

        while (caracteresIgnorados < segmento.Length &&
            (char.IsWhiteSpace(segmento[caracteresIgnorados]) ||
             segmento[caracteresIgnorados] is ':' or '=' or '-' or '–' or '—')) {
            caracteresIgnorados++;
        }

        string valorNormalizado = NormalizarTexto(segmento[caracteresIgnorados..]);

        foreach ((string representacion, string valor) in
            CrearRepresentacionesTextuales(esperado)) {
            string representacionNormalizada = NormalizarTexto(representacion);

            if (representacionNormalizada.Length == 0 ||
                !CoincideValorTextual(valorNormalizado, representacionNormalizada)) {
                continue;
            }

            return new CandidatoTexto(
                indiceLinea,
                desplazamiento + caracteresIgnorados,
                representacion.Length,
                valor,
                etiqueta,
                representacion);
        }

        return null;
    }

    private static IReadOnlyList<(string Representacion, string Valor)>
        CrearRepresentacionesTextuales(ValorTextualEsperado esperado) {
        Dictionary<string, (string Representacion, string Valor)> representaciones =
            new(StringComparer.Ordinal);

        foreach (OpcionValorTextual opcion in esperado.Opciones) {
            foreach (string representacion in opcion.Alternativas) {
                string normalizada = NormalizarTexto(representacion);

                if (normalizada.Length > 0) {
                    representaciones.TryAdd(
                        normalizada,
                        (representacion, opcion.Valor));
                }
            }
        }

        return representaciones.Values
            .OrderByDescending(elemento => elemento.Representacion.Length)
            .ToArray();
    }

    private static bool CoincideValorTextual(string texto, string valor) {
        return texto.Equals(valor, StringComparison.Ordinal) ||
            texto.StartsWith(valor + ".", StringComparison.Ordinal) ||
            texto.StartsWith(valor + ",", StringComparison.Ordinal) ||
            texto.StartsWith(valor + ";", StringComparison.Ordinal);
    }

    private static bool EmpiezaConToken(string texto, string token) {
        return texto.Equals(token, StringComparison.Ordinal) ||
            texto.StartsWith(token + " ", StringComparison.Ordinal) ||
            texto.StartsWith(token + ".", StringComparison.Ordinal) ||
            texto.StartsWith(token + ",", StringComparison.Ordinal) ||
            texto.StartsWith(token + ";", StringComparison.Ordinal);
    }

    private static IReadOnlyList<string> CrearEtiquetasBusqueda(
        ValorNumericoEsperado esperado) {
        List<string> etiquetas = new() { esperado.Nombre };
        etiquetas.AddRange(esperado.EtiquetasAlternativas);

        return etiquetas
            .Where(etiqueta => !string.IsNullOrWhiteSpace(etiqueta))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(etiqueta => etiqueta.Length)
            .ToArray();
    }

    private static IReadOnlyList<string> CrearEtiquetasBusqueda(
        ValorBooleanoEsperado esperado) {
        return new[] { esperado.Nombre }
            .Concat(esperado.EtiquetasAlternativas)
            .Where(etiqueta => !string.IsNullOrWhiteSpace(etiqueta))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(etiqueta => etiqueta.Length)
            .ToArray();
    }

    private static IReadOnlyList<string> CrearEtiquetasBusqueda(
        ValorTextualEsperado esperado) {
        return new[] { esperado.Nombre }
            .Concat(esperado.EtiquetasAlternativas)
            .Where(etiqueta => !string.IsNullOrWhiteSpace(etiqueta))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(etiqueta => etiqueta.Length)
            .ToArray();
    }

    private static ResultadoValorNumericoComparado CrearResultadoValor(
        ValorNumericoEsperado esperado,
        double? valorObtenido,
        bool coincide,
        string etiquetaEncontrada,
        bool tieneContradiccion,
        IReadOnlyList<double> valoresEncontrados) {
        return new ResultadoValorNumericoComparado {
            Nombre = esperado.Nombre,
            ValorEsperado = esperado.Valor,
            ValorObtenido = valorObtenido,
            Tolerancia = esperado.Tolerancia,
            EsOpcional = esperado.EsOpcional,
            DebeEstarAusente = esperado.DebeEstarAusente,
            Coincide = coincide,
            EtiquetaEncontrada = etiquetaEncontrada,
            TieneContradiccion = tieneContradiccion,
            UsoEtiquetaAlternativa = EsEtiquetaAlternativa(
                etiquetaEncontrada,
                esperado.EtiquetasAlternativas),
            ValoresEncontrados = valoresEncontrados
        };
    }

    private static bool EsEtiquetaAlternativa(
        string etiqueta,
        IReadOnlyList<string> alternativas) {
        return alternativas.Any(alternativa =>
            string.Equals(alternativa, etiqueta, StringComparison.OrdinalIgnoreCase));
    }

    private static bool CoincideValorNumericoEsperado(
        double obtenido,
        ValorNumericoEsperado esperado) {
        return SonEquivalentes(obtenido, esperado.Valor, esperado.Tolerancia) ||
            esperado.ValoresEquivalentes.Any(valor =>
                SonEquivalentes(obtenido, valor, esperado.Tolerancia));
    }

    private static bool SonRepresentacionesNumericasEquivalentes(
        double primero,
        double segundo,
        ValorNumericoEsperado esperado) {
        return SonEquivalentes(primero, segundo, esperado.Tolerancia) ||
            CoincideValorNumericoEsperado(primero, esperado) &&
            CoincideValorNumericoEsperado(segundo, esperado);
    }

    private static bool SonEquivalentes(double obtenido, double esperado, double tolerancia) {
        return double.IsFinite(obtenido) &&
            double.IsFinite(esperado) &&
            Math.Abs(obtenido - esperado) <= Math.Max(0D, tolerancia);
    }

    private static bool IntentarConvertirNumero(string texto, out double valor) {
        string normalizado = texto.Trim();

        if (normalizado.StartsWith('$')) {
            normalizado = normalizado[1..].TrimStart();
        }

        normalizado = normalizado.Replace(',', '.');
        return double.TryParse(
            normalizado,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out valor);
    }

    private static bool ContieneToken(string textoNormalizado, string token) {
        string tokenNormalizado = NormalizarTexto(token);
        return tokenNormalizado.Length > 0 &&
            CrearRegexToken(tokenNormalizado).IsMatch(textoNormalizado);
    }

    private static bool CumpleGrupoAlternativo(
        string salida,
        string salidaNormalizada,
        GrupoTokensEsperados grupo) {
        if (grupo.EtiquetasAsociadas.Count == 0) {
            return grupo.Alternativas.Any(alternativa =>
                ContieneToken(salidaNormalizada, alternativa));
        }

        string[] lineas = SepararLineas(salida);

        foreach (string etiqueta in grupo.EtiquetasAsociadas) {
            string etiquetaNormalizada = NormalizarTexto(etiqueta);

            if (etiquetaNormalizada.Length == 0) {
                continue;
            }

            for (int indiceLinea = 0; indiceLinea < lineas.Length; indiceLinea++) {
                string lineaNormalizada = NormalizarLineaPreservandoIndices(
                    lineas[indiceLinea]);

                foreach (Match coincidencia in CrearRegexToken(etiquetaNormalizada)
                    .Matches(lineaNormalizada)
                    .Cast<Match>()) {
                    string segmento = lineaNormalizada[
                        Math.Min(coincidencia.Index + coincidencia.Length, lineaNormalizada.Length)..];

                    if (EmpiezaConAlternativa(segmento, grupo.Alternativas)) {
                        return true;
                    }

                    for (int siguiente = indiceLinea + 1; siguiente < lineas.Length; siguiente++) {
                        string siguienteLinea = NormalizarTexto(lineas[siguiente]);

                        if (siguienteLinea.Length == 0) {
                            continue;
                        }

                        if (EmpiezaConAlternativa(
                            siguienteLinea,
                            grupo.Alternativas)) {
                            return true;
                        }

                        break;
                    }
                }
            }
        }

        return false;
    }

    private static bool EmpiezaConAlternativa(
        string segmento,
        IReadOnlyList<string> alternativas) {
        string valor = NormalizarTexto(segmento)
            .TrimStart(' ', ':', '=', '-', '–', '—');

        foreach (string alternativa in alternativas) {
            string alternativaNormalizada = NormalizarTexto(alternativa);

            if (alternativaNormalizada.Length == 0) {
                continue;
            }

            if (valor.Equals(alternativaNormalizada, StringComparison.Ordinal) ||
                valor.StartsWith(
                    alternativaNormalizada + " ",
                    StringComparison.Ordinal)) {
                return true;
            }
        }

        return false;
    }

    private static Regex CrearRegexToken(string tokenNormalizado) {
        string[] partes = tokenNormalizado
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string cuerpo = string.Join(@"\s+", partes.Select(Regex.Escape));

        return new Regex(
            $@"(?<![\p{{L}}\p{{N}}_]){cuerpo}(?![\p{{L}}\p{{N}}_])",
            RegexOptions.CultureInvariant);
    }

    private static string NormalizarTexto(string texto) {
        string descompuesto = texto.Normalize(NormalizationForm.FormD);
        StringBuilder resultado = new(descompuesto.Length);
        bool espacioPendiente = false;

        foreach (char caracter in descompuesto) {
            UnicodeCategory categoria = CharUnicodeInfo.GetUnicodeCategory(caracter);

            if (categoria == UnicodeCategory.NonSpacingMark) {
                continue;
            }

            if (char.IsWhiteSpace(caracter)) {
                espacioPendiente = resultado.Length > 0;
                continue;
            }

            if (espacioPendiente) {
                resultado.Append(' ');
                espacioPendiente = false;
            }

            resultado.Append(char.ToLowerInvariant(caracter));
        }

        return resultado.ToString().Trim();
    }

    private static string NormalizarLineaPreservandoIndices(string texto) {
        string descompuesto = texto.Normalize(NormalizationForm.FormD);
        StringBuilder resultado = new(texto.Length);

        foreach (char caracter in descompuesto) {
            if (CharUnicodeInfo.GetUnicodeCategory(caracter) ==
                UnicodeCategory.NonSpacingMark) {
                continue;
            }

            resultado.Append(char.ToLowerInvariant(caracter));
        }

        return resultado.ToString();
    }

    private static string[] SepararLineas(string texto) {
        return texto
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');
    }

    private static bool EsLegible(
        string salida,
        int tokensEncontrados,
        int totalTokens,
        bool contieneSecuenciaReconocible) {
        if (string.IsNullOrWhiteSpace(salida) || salida.IndexOf('\0') >= 0) {
            return false;
        }

        int caracteresVisibles = 0;
        int controlesInvalidos = 0;

        foreach (char caracter in salida) {
            if (!char.IsWhiteSpace(caracter)) {
                caracteresVisibles++;
            }

            if (char.IsControl(caracter) && caracter is not '\r' and not '\n' and not '\t') {
                controlesInvalidos++;
            }
        }

        int minimoTokens = totalTokens == 0
            ? 0
            : Math.Max(1, (int)Math.Ceiling(totalTokens / 2D));
        bool tieneEstructuraVisual =
            salida.Contains(':') ||
            salida.Contains('=') ||
            salida.Contains('\n') ||
            salida.Contains('\r') ||
            contieneSecuenciaReconocible;

        return caracteresVisibles >= 3 &&
            controlesInvalidos == 0 &&
            tokensEncontrados >= minimoTokens &&
            tieneEstructuraVisual;
    }

    private static List<string> CrearReglasCumplidas(
        CasoPrueba caso,
        bool compararTexto,
        bool compararValores,
        IReadOnlyList<string> tokensFaltantes,
        IReadOnlyList<string> gruposFaltantes,
        IReadOnlyList<ResultadoValorNumericoComparado> valores,
        IReadOnlyList<ResultadoValorBooleanoComparado> booleanos,
        IReadOnlyList<ResultadoValorTextualComparado> textos,
        IReadOnlyList<ResultadoSecuenciaComparada> secuencias,
        IReadOnlyList<ResultadoSecuenciaCompuestaComparada> secuenciasCompuestas) {
        List<string> reglas = new();

        if (compararTexto) {
            reglas.AddRange(caso.TokensObligatorios
                .Where(token => !tokensFaltantes.Contains(
                    token,
                    StringComparer.OrdinalIgnoreCase))
                .Select(token => $"Texto obligatorio: {token}"));
            reglas.AddRange(caso.GruposTokensAlternativos
                .Where(grupo => !gruposFaltantes.Contains(
                    grupo.Nombre,
                    StringComparer.OrdinalIgnoreCase))
                .Select(grupo => $"Alternativa textual: {grupo.Nombre}"));
            reglas.AddRange(textos
                .Where(valor =>
                    valor.Coincide &&
                    !valor.TieneContradiccion &&
                    (!valor.EsOpcional ||
                     !string.IsNullOrWhiteSpace(valor.EtiquetaEncontrada)))
                .Select(valor => $"Valor textual correcto: {valor.Nombre}"));
        }

        if (compararValores) {
            reglas.AddRange(valores
                .Where(valor =>
                    valor.Coincide &&
                    !valor.TieneContradiccion &&
                    !valor.DebeEstarAusente &&
                    (!valor.EsOpcional ||
                     !string.IsNullOrWhiteSpace(valor.EtiquetaEncontrada)))
                .Select(valor => $"Valor correcto: {valor.Nombre}"));
            reglas.AddRange(booleanos
                .Where(valor => valor.Coincide && !valor.TieneContradiccion)
                .Select(valor => $"Valor booleano correcto: {valor.Nombre}"));
        }

        reglas.AddRange(secuencias
            .Where(secuencia => secuencia.Coincide)
            .Select(secuencia => $"Secuencia correcta: {secuencia.Nombre}"));
        reglas.AddRange(secuenciasCompuestas
            .Where(secuencia => secuencia.Coincide)
            .Select(secuencia => $"Secuencia de filas correcta: {secuencia.Nombre}"));

        return reglas;
    }

    private static List<string> CrearReglasIncumplidas(
        CasoPrueba caso,
        bool compararTexto,
        bool compararValores,
        IReadOnlyList<string> tokensFaltantes,
        IReadOnlyList<string> gruposFaltantes,
        IReadOnlyList<ResultadoValorNumericoComparado> valores,
        IReadOnlyList<ResultadoValorBooleanoComparado> booleanos,
        IReadOnlyList<ResultadoValorTextualComparado> textos,
        IReadOnlyList<ResultadoSecuenciaComparada> secuencias,
        IReadOnlyList<ResultadoSecuenciaCompuestaComparada> secuenciasCompuestas) {
        List<string> reglas = new();

        if (compararTexto) {
            reglas.AddRange(tokensFaltantes.Select(token =>
                $"Falta el texto obligatorio: {token}"));
            reglas.AddRange(gruposFaltantes.Select(grupo =>
                $"Falta una alternativa textual válida: {grupo}"));
            reglas.AddRange(textos
                .Where(valor => !valor.Coincide)
                .Select(valor => valor.TieneContradiccion
                    ? $"Valores textuales contradictorios: {valor.Nombre}"
                    : $"Valor textual incorrecto o ausente: {valor.Nombre}"));
        }

        if (compararValores) {
            reglas.AddRange(valores
                .Where(valor => !valor.Coincide)
                .Select(valor => valor.TieneContradiccion
                    ? $"Valores contradictorios: {valor.Nombre}"
                    : valor.DebeEstarAusente
                        ? $"No debe mostrarse un valor numérico para: {valor.Nombre}"
                        : $"Valor incorrecto o ausente: {valor.Nombre}"));
            reglas.AddRange(booleanos
                .Where(valor => !valor.Coincide)
                .Select(valor => valor.TieneContradiccion
                    ? $"Valores booleanos contradictorios: {valor.Nombre}"
                    : $"Valor booleano incorrecto o ausente: {valor.Nombre}"));

            if (caso.ModoComparacion == ModoComparacionCaso.Valores &&
                valores.Count == 0 &&
                booleanos.Count == 0) {
                reglas.Add("El caso no tiene valores configurados para comparar.");
            }
        }

        reglas.AddRange(secuencias
            .Where(secuencia => !secuencia.Coincide)
            .Select(secuencia => $"Secuencia incorrecta: {secuencia.Nombre}"));
        reglas.AddRange(secuenciasCompuestas
            .Where(secuencia => !secuencia.Coincide)
            .Select(secuencia =>
                $"Secuencia de filas incorrecta: {secuencia.Nombre}"));

        return reglas;
    }

    private static string CrearMensaje(
        string salida,
        bool esCorrecta,
        bool cumpleEstructura,
        IReadOnlyList<ResultadoValorNumericoComparado> valores,
        IReadOnlyList<ResultadoValorBooleanoComparado> booleanos,
        IReadOnlyList<ResultadoValorTextualComparado> textos,
        IReadOnlyList<ResultadoSecuenciaComparada> secuencias,
        IReadOnlyList<ResultadoSecuenciaCompuestaComparada> secuenciasCompuestas,
        IReadOnlyList<string> contradicciones) {
        if (string.IsNullOrWhiteSpace(salida)) {
            return "El programa terminó sin mostrar una salida que pueda evaluarse.";
        }

        if (esCorrecta) {
            return "La salida contiene los datos y cálculos esperados.";
        }

        if (contradicciones.Count > 0) {
            return $"La salida contiene valores contradictorios para: {string.Join(", ", contradicciones)}.";
        }

        ResultadoSecuenciaComparada? secuenciaIncorrecta = secuencias
            .FirstOrDefault(secuencia => !secuencia.Coincide);

        if (secuenciaIncorrecta is not null) {
            return secuenciaIncorrecta.Mensaje;
        }

        ResultadoSecuenciaCompuestaComparada? secuenciaCompuestaIncorrecta =
            secuenciasCompuestas.FirstOrDefault(secuencia => !secuencia.Coincide);

        if (secuenciaCompuestaIncorrecta is not null) {
            return secuenciaCompuestaIncorrecta.Mensaje;
        }

        if (!cumpleEstructura) {
            return "La salida necesita identificar con mayor claridad todos los datos solicitados.";
        }

        string[] valoresIncorrectos = valores
            .Where(valor => !valor.Coincide)
            .Select(valor => valor.Nombre)
            .Concat(booleanos
                .Where(valor => !valor.Coincide)
                .Select(valor => valor.Nombre))
            .Concat(textos
                .Where(valor => !valor.Coincide)
                .Select(valor => valor.Nombre))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return valoresIncorrectos.Length > 0
            ? $"Revisa el cálculo o la etiqueta de: {string.Join(", ", valoresIncorrectos)}."
            : "La salida todavía necesita algunos ajustes.";
    }

    [GeneratedRegex(@"^\s*\$?\s*[-+]?(?:\d+(?:[.,]\d+)?|[.,]\d+)(?![\p{L}\p{N}_])", RegexOptions.CultureInvariant)]
    private static partial Regex RegexNumeroInicial();

    [GeneratedRegex(@"(?<![\p{L}\p{N}_])[-+]?(?:\d+(?:\.\d+)?|\.\d+)(?![\p{L}\p{N}_])", RegexOptions.CultureInvariant)]
    private static partial Regex RegexNumeroSecuenciaConComaSeparador();

    [GeneratedRegex(@"(?<![\p{L}\p{N}_])[-+]?(?:\d+(?:[.,]\d+)?|[.,]\d+)(?![\p{L}\p{N}_])", RegexOptions.CultureInvariant)]
    private static partial Regex RegexNumeroSecuenciaConComaDecimal();

    private readonly record struct PosicionNumero(
        int IndiceLinea,
        int IndiceCaracter,
        int Longitud);

    private sealed record CandidatoNumero(
        int IndiceLinea,
        int IndiceCaracter,
        int Longitud,
        double Valor,
        string Etiqueta);

    private sealed record CandidatoBooleano(
        int IndiceLinea,
        int IndiceCaracter,
        int Longitud,
        bool Valor,
        string Etiqueta,
        string Representacion);

    private sealed record CandidatoTexto(
        int IndiceLinea,
        int IndiceCaracter,
        int Longitud,
        string Valor,
        string Etiqueta,
        string Representacion);

    private sealed record CandidatoSecuenciaNumerica(
        int Indice,
        int Longitud,
        double Valor);

    private sealed record CandidatoSecuenciaTextual(
        int Indice,
        int Longitud,
        string Valor);

    private readonly record struct IntervaloSecuencia(
        int Indice,
        int Longitud);

    private sealed record PasoCompuestoEncontrado(
        int NumeroLinea,
        string Representacion,
        IReadOnlyList<double> Valores);

    private sealed record FilaCompuestaInvalida(
        int NumeroLinea,
        string Representacion);
}
