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

        bool cumpleTexto =
            tokensFaltantes.Count == 0 &&
            gruposFaltantes.Count == 0 &&
            textosComparados.All(valor => valor.Coincide);
        bool hayReglasValores =
            valoresComparados.Count > 0 || booleanosComparados.Count > 0;
        bool valoresCorrectos = caso.ModoComparacion switch {
            ModoComparacionCaso.Texto => true,
            ModoComparacionCaso.Valores =>
                hayReglasValores &&
                valoresComparados.All(valor => valor.Coincide) &&
                booleanosComparados.All(valor => valor.Coincide),
            _ =>
                !hayReglasValores ||
                valoresComparados.All(valor => valor.Coincide) &&
                booleanosComparados.All(valor => valor.Coincide)
        };
        bool etiquetasValoresPresentes = !compararValores ||
            valoresComparados.All(valor => !string.IsNullOrWhiteSpace(valor.EtiquetaEncontrada)) &&
            booleanosComparados.All(valor => !string.IsNullOrWhiteSpace(valor.EtiquetaEncontrada));
        bool etiquetasTextoPresentes = !compararTexto ||
            textosComparados.All(valor => !string.IsNullOrWhiteSpace(valor.EtiquetaEncontrada));
        bool cumpleEstructura =
            cumpleTexto && etiquetasValoresPresentes && etiquetasTextoPresentes;
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
            textosComparados);
        List<string> reglasIncumplidas = CrearReglasIncumplidas(
            caso,
            compararTexto,
            compararValores,
            tokensFaltantes,
            gruposFaltantes,
            valoresComparados,
            booleanosComparados,
            textosComparados);
        bool salidaLegible = EsLegible(
            salida,
            compararTexto
                ? caso.TokensObligatorios.Count - tokensFaltantes.Count
                : 0,
            compararTexto ? caso.TokensObligatorios.Count : 0);
        bool esCorrecta = cumpleTexto && valoresCorrectos && contradicciones.Count == 0;

        return new ResultadoComparacionSalida {
            EsCorrecta = esCorrecta,
            CumpleEstructura = cumpleEstructura,
            EsSalidaLegible = salidaLegible,
            TokensFaltantes = tokensFaltantes.AsReadOnly(),
            GruposAlternativosFaltantes = gruposFaltantes.AsReadOnly(),
            ValoresNumericos = valoresComparados.AsReadOnly(),
            ValoresBooleanos = booleanosComparados.AsReadOnly(),
            ValoresTextuales = textosComparados.AsReadOnly(),
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
                contradicciones)
        };
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
            encontrados.Any(otro => !SonEquivalentes(
                candidato.Valor,
                otro.Valor,
                esperado.Tolerancia)));
        bool coincide = encontrados.Length > 0 &&
            !tieneContradiccion &&
            encontrados.All(candidato => SonEquivalentes(
                candidato.Valor,
                esperado.Valor,
                esperado.Tolerancia));
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
        bool coincide = encontrados.Length > 0 &&
            !tieneContradiccion &&
            encontrados.All(candidato => candidato.Valor.Equals(
                esperado.Valor,
                StringComparison.OrdinalIgnoreCase));
        CandidatoTexto? primerCandidato = encontrados.FirstOrDefault();

        return new ResultadoValorTextualComparado {
            Nombre = esperado.Nombre,
            ValorEsperado = esperado.Valor,
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

    private static bool SonEquivalentes(double obtenido, double esperado, double tolerancia) {
        return double.IsFinite(obtenido) &&
            double.IsFinite(esperado) &&
            Math.Abs(obtenido - esperado) <= Math.Max(0D, tolerancia);
    }

    private static bool IntentarConvertirNumero(string texto, out double valor) {
        string normalizado = texto.Trim().Replace(',', '.');
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
        int totalTokens) {
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
            salida.Contains('\r');

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
        IReadOnlyList<ResultadoValorTextualComparado> textos) {
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
                .Where(valor => valor.Coincide && !valor.TieneContradiccion)
                .Select(valor => $"Valor textual correcto: {valor.Nombre}"));
        }

        if (compararValores) {
            reglas.AddRange(valores
                .Where(valor => valor.Coincide && !valor.TieneContradiccion)
                .Select(valor => $"Valor correcto: {valor.Nombre}"));
            reglas.AddRange(booleanos
                .Where(valor => valor.Coincide && !valor.TieneContradiccion)
                .Select(valor => $"Valor booleano correcto: {valor.Nombre}"));
        }

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
        IReadOnlyList<ResultadoValorTextualComparado> textos) {
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

        return reglas;
    }

    private static string CrearMensaje(
        string salida,
        bool esCorrecta,
        bool cumpleEstructura,
        IReadOnlyList<ResultadoValorNumericoComparado> valores,
        IReadOnlyList<ResultadoValorBooleanoComparado> booleanos,
        IReadOnlyList<ResultadoValorTextualComparado> textos,
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

    [GeneratedRegex(@"^\s*[-+]?(?:\d+(?:[.,]\d+)?|[.,]\d+)(?![\p{L}\p{N}_])", RegexOptions.CultureInvariant)]
    private static partial Regex RegexNumeroInicial();

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
}
