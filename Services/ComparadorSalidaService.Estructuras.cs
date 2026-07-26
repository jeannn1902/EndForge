using EndForge.Models;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace EndForge.Services;

public sealed partial class ComparadorSalidaService {
    private static ResultadoReglasEstructuradas CompararReglasEstructuradas(
        CasoPrueba caso,
        string salida) {
        List<ResultadoColeccionComparada> colecciones = caso.ColeccionesEsperadas
            .Select(regla => CompararColeccion(salida, regla))
            .ToList();
        List<ResultadoCadenaComparada> cadenas = caso.CadenasEsperadas
            .Select(regla => CompararCadena(salida, regla))
            .ToList();
        List<ResultadoTablaComparada> tablas = caso.TablasEsperadas
            .Select(regla => CompararTabla(salida, regla))
            .ToList();
        List<ResultadoMatrizComparada> matrices = caso.MatricesEsperadas
            .Select(regla => CompararMatriz(salida, regla))
            .ToList();
        List<ResultadoBloquesRegistroComparados> bloques =
            caso.BloquesRegistroEsperados
                .Select(regla => CompararBloquesRegistro(salida, regla))
                .ToList();

        bool coincide =
            colecciones.All(resultado => resultado.Coincide) &&
            cadenas.All(resultado => resultado.Coincide) &&
            tablas.All(resultado => resultado.Coincide) &&
            matrices.All(resultado => resultado.Coincide) &&
            bloques.All(resultado => resultado.Coincide);
        bool tieneReglas =
            colecciones.Count + cadenas.Count + tablas.Count +
            matrices.Count + bloques.Count > 0;
        bool tieneEstructuraReconocible =
            colecciones.Any(resultado => resultado.RegionEncontrada) ||
            cadenas.Any(resultado => resultado.EtiquetaPresente ||
                resultado.ValoresEncontrados.Count > 0) ||
            tablas.Any(resultado => resultado.CantidadEncontrada > 0) ||
            matrices.Any(resultado => resultado.FilasEncontradas > 0) ||
            bloques.Any(resultado => resultado.CantidadEncontrada > 0);

        List<string> contradicciones = colecciones
            .Where(resultado => resultado.TieneContradiccion)
            .Select(resultado => resultado.Nombre)
            .Concat(cadenas
                .Where(resultado => resultado.TieneContradiccion)
                .Select(resultado => resultado.Nombre))
            .Concat(tablas
                .Where(resultado => resultado.TieneContradiccion)
                .Select(resultado => resultado.Nombre))
            .Concat(matrices
                .Where(resultado => resultado.TieneContradiccion)
                .Select(resultado => resultado.Nombre))
            .Concat(bloques
                .Where(resultado => resultado.TieneContradiccion)
                .Select(resultado => resultado.Nombre))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        List<string> reglasCumplidas = new();
        reglasCumplidas.AddRange(colecciones
            .Where(resultado => resultado.Coincide)
            .Select(resultado => $"Colección correcta: {resultado.Nombre}"));
        reglasCumplidas.AddRange(cadenas
            .Where(resultado => resultado.Coincide)
            .Select(resultado => $"Cadena correcta: {resultado.Nombre}"));
        reglasCumplidas.AddRange(tablas
            .Where(resultado => resultado.Coincide)
            .Select(resultado => $"Tabla correcta: {resultado.Nombre}"));
        reglasCumplidas.AddRange(matrices
            .Where(resultado => resultado.Coincide)
            .Select(resultado => $"Matriz correcta: {resultado.Nombre}"));
        reglasCumplidas.AddRange(bloques
            .Where(resultado => resultado.Coincide)
            .Select(resultado => $"Bloques de registros correctos: {resultado.Nombre}"));

        List<string> reglasIncumplidas = new();
        reglasIncumplidas.AddRange(colecciones
            .Where(resultado => !resultado.Coincide)
            .Select(resultado => caso.EsVisible
                ? $"Colección incorrecta: {resultado.Nombre}"
                : "Una colección del caso oculto no coincide."));
        reglasIncumplidas.AddRange(cadenas
            .Where(resultado => !resultado.Coincide)
            .Select(resultado => caso.EsVisible
                ? $"Cadena incorrecta: {resultado.Nombre}"
                : "Una cadena del caso oculto no coincide."));
        reglasIncumplidas.AddRange(tablas
            .Where(resultado => !resultado.Coincide)
            .Select(resultado => caso.EsVisible
                ? $"Tabla incorrecta: {resultado.Nombre}"
                : "Una tabla del caso oculto no coincide."));
        reglasIncumplidas.AddRange(matrices
            .Where(resultado => !resultado.Coincide)
            .Select(resultado => caso.EsVisible
                ? $"Matriz incorrecta: {resultado.Nombre}"
                : "Una matriz del caso oculto no coincide."));
        reglasIncumplidas.AddRange(bloques
            .Where(resultado => !resultado.Coincide)
            .Select(resultado => caso.EsVisible
                ? $"Bloques de registros incorrectos: {resultado.Nombre}"
                : "Un bloque de registros del caso oculto no coincide."));

        string mensaje = coincide
            ? string.Empty
            : caso.EsVisible
                ? ObtenerPrimerMensajeEstructurado(
                    colecciones,
                    cadenas,
                    tablas,
                    matrices,
                    bloques)
                : "La salida no cumple una regla estructural del caso oculto.";

        return new ResultadoReglasEstructuradas(
            tieneReglas,
            coincide,
            tieneEstructuraReconocible,
            colecciones.AsReadOnly(),
            cadenas.AsReadOnly(),
            tablas.AsReadOnly(),
            matrices.AsReadOnly(),
            bloques.AsReadOnly(),
            reglasCumplidas.AsReadOnly(),
            reglasIncumplidas.AsReadOnly(),
            contradicciones.AsReadOnly(),
            mensaje);
    }

    private static string ObtenerPrimerMensajeEstructurado(
        IReadOnlyList<ResultadoColeccionComparada> colecciones,
        IReadOnlyList<ResultadoCadenaComparada> cadenas,
        IReadOnlyList<ResultadoTablaComparada> tablas,
        IReadOnlyList<ResultadoMatrizComparada> matrices,
        IReadOnlyList<ResultadoBloquesRegistroComparados> bloques) {
        return colecciones.FirstOrDefault(resultado => !resultado.Coincide)?.Mensaje ??
            cadenas.FirstOrDefault(resultado => !resultado.Coincide)?.Mensaje ??
            tablas.FirstOrDefault(resultado => !resultado.Coincide)?.Mensaje ??
            matrices.FirstOrDefault(resultado => !resultado.Coincide)?.Mensaje ??
            bloques.FirstOrDefault(resultado => !resultado.Coincide)?.Mensaje ??
            "La salida no coincide con la estructura esperada.";
    }

    private static IReadOnlyList<LineaEstructurada> SepararLineasEstructuradas(
        string texto) {
        string normalizado = texto
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        string[] lineas = normalizado.Split('\n');
        List<LineaEstructurada> resultado = new(lineas.Length);
        int indice = 0;

        for (int numero = 0; numero < lineas.Length; numero++) {
            resultado.Add(new LineaEstructurada(numero, indice, lineas[numero]));
            indice += lineas[numero].Length + 1;
        }

        return resultado.AsReadOnly();
    }

    private static IReadOnlyList<RegionEstructurada> ExtraerRegiones(
        string salida,
        IReadOnlyList<string> etiquetasInicio,
        IReadOnlyList<string> etiquetasFin,
        ModoRegionColeccion modo,
        bool requerirEtiqueta) {
        IReadOnlyList<LineaEstructurada> lineas = SepararLineasEstructuradas(salida);

        if (etiquetasInicio.Count == 0) {
            return requerirEtiqueta
                ? Array.Empty<RegionEstructurada>()
                : Array.AsReadOnly(new[] {
                    new RegionEstructurada(
                        string.Empty,
                        0,
                        lineas,
                        salida)
                });
        }

        List<RegionEstructurada> regiones = new();

        for (int indiceLinea = 0; indiceLinea < lineas.Count; indiceLinea++) {
            LineaEstructurada linea = lineas[indiceLinea];

            if (!IntentarExtraerTrasEtiqueta(
                linea.Texto,
                etiquetasInicio,
                out string etiqueta,
                out string resto)) {
                continue;
            }

            List<LineaEstructurada> lineasRegion = new();

            if (modo == ModoRegionColeccion.MismaLineaTrasEtiqueta) {
                lineasRegion.Add(new LineaEstructurada(
                    linea.Numero,
                    linea.IndiceInicio + Math.Max(0, linea.Texto.Length - resto.Length),
                    resto));
            } else {
                if (!string.IsNullOrWhiteSpace(resto)) {
                    lineasRegion.Add(new LineaEstructurada(
                        linea.Numero,
                        linea.IndiceInicio + Math.Max(0, linea.Texto.Length - resto.Length),
                        resto));
                }

                for (int siguiente = indiceLinea + 1;
                     siguiente < lineas.Count;
                     siguiente++) {
                    LineaEstructurada candidata = lineas[siguiente];

                    if (CoincideAlgunaEtiqueta(candidata.Texto, etiquetasFin) ||
                        CoincideAlgunaEtiqueta(candidata.Texto, etiquetasInicio)) {
                        break;
                    }

                    if (modo == ModoRegionColeccion.BloqueHastaLineaVacia &&
                        string.IsNullOrWhiteSpace(candidata.Texto)) {
                        break;
                    }

                    lineasRegion.Add(candidata);
                }
            }

            string contenido = string.Join(
                Environment.NewLine,
                lineasRegion.Select(item => item.Texto));
            regiones.Add(new RegionEstructurada(
                etiqueta,
                linea.Numero,
                lineasRegion.AsReadOnly(),
                contenido));
        }

        if (regiones.Count == 0 && !requerirEtiqueta) {
            regiones.Add(new RegionEstructurada(
                string.Empty,
                0,
                lineas,
                salida));
        }

        return regiones.AsReadOnly();
    }

    private static bool IntentarExtraerTrasEtiqueta(
        string linea,
        IReadOnlyList<string> etiquetas,
        out string etiquetaEncontrada,
        out string resto,
        bool preservarEspaciosValor = false) {
        string candidata = linea.TrimStart().Normalize(NormalizationForm.FormC);

        foreach (string etiquetaOriginal in etiquetas
            .Where(etiqueta => !string.IsNullOrWhiteSpace(etiqueta))
            .OrderByDescending(etiqueta => etiqueta.Length)) {
            string etiqueta = etiquetaOriginal.Trim().Normalize(NormalizationForm.FormC);

            if (candidata.Length < etiqueta.Length ||
                !string.Equals(
                    NormalizarBusquedaEstructurada(candidata[..etiqueta.Length]),
                    NormalizarBusquedaEstructurada(etiqueta),
                    StringComparison.Ordinal)) {
                continue;
            }

            string restante = candidata[etiqueta.Length..];

            if (restante.Length > 0 &&
                !char.IsWhiteSpace(restante[0]) &&
                restante[0] is not ':' and not '=') {
                continue;
            }

            int cursor = 0;

            while (cursor < restante.Length &&
                   char.IsWhiteSpace(restante[cursor])) {
                cursor++;
            }

            int espaciosSeparacion = cursor;
            bool tieneSeparador = cursor < restante.Length &&
                (restante[cursor] is ':' or '=' ||
                 restante[cursor] == '-' &&
                 cursor + 1 < restante.Length &&
                 char.IsWhiteSpace(restante[cursor + 1]));

            if (tieneSeparador) {
                cursor++;

                if (preservarEspaciosValor &&
                    cursor < restante.Length &&
                    char.IsWhiteSpace(restante[cursor])) {
                    cursor++;
                }
            } else if (preservarEspaciosValor && espaciosSeparacion > 0) {
                cursor = 1;
            }

            etiquetaEncontrada = etiquetaOriginal;
            resto = preservarEspaciosValor
                ? restante[cursor..]
                : restante[cursor..].TrimStart();
            return true;
        }

        etiquetaEncontrada = string.Empty;
        resto = string.Empty;
        return false;
    }

    private static bool CoincideAlgunaEtiqueta(
        string linea,
        IReadOnlyList<string> etiquetas) {
        return IntentarExtraerTrasEtiqueta(
            linea,
            etiquetas,
            out _,
            out _);
    }

    private static string NormalizarBusquedaEstructurada(string texto) {
        string descompuesto = texto
            .Normalize(NormalizationForm.FormD);
        StringBuilder resultado = new(descompuesto.Length);

        foreach (char caracter in descompuesto) {
            if (CharUnicodeInfo.GetUnicodeCategory(caracter) !=
                UnicodeCategory.NonSpacingMark) {
                resultado.Append(char.ToLowerInvariant(caracter));
            }
        }

        return resultado
            .ToString()
            .Normalize(NormalizationForm.FormC);
    }

    private static string NormalizarCadenaEstructurada(
        string texto,
        bool distinguirMayusculas,
        bool distinguirAcentos,
        PoliticaEspaciosCadena politicaEspacios) {
        string resultado = texto.Normalize(NormalizationForm.FormC);

        if (!distinguirAcentos) {
            string descompuesto = resultado.Normalize(NormalizationForm.FormD);
            StringBuilder sinAcentos = new(descompuesto.Length);

            foreach (char caracter in descompuesto) {
                if (CharUnicodeInfo.GetUnicodeCategory(caracter) !=
                    UnicodeCategory.NonSpacingMark) {
                    sinAcentos.Append(caracter);
                }
            }

            resultado = sinAcentos
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }

        if (!distinguirMayusculas) {
            resultado = resultado.ToLowerInvariant();
        }

        return politicaEspacios switch {
            PoliticaEspaciosCadena.RecortarExtremos => resultado.Trim(),
            PoliticaEspaciosCadena.ColapsarInternos =>
                Regex.Replace(resultado.Trim(), @"\s+", " "),
            _ => resultado
        };
    }

    private static bool IntentarConvertirNumeroEstructurado(
        string texto,
        out double valor) {
        string candidato = texto.Trim();

        if (double.TryParse(
            candidato,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out valor)) {
            return double.IsFinite(valor);
        }

        candidato = candidato.Replace(',', '.');
        return double.TryParse(
                candidato,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out valor) &&
            double.IsFinite(valor);
    }

    private static IReadOnlyList<string> ExtraerElementosColeccion(
        string contenido,
        TipoValorEstructurado tipo,
        IReadOnlyList<string> separadores) {
        if (tipo == TipoValorEstructurado.Numerico) {
            string preparado = contenido;

            foreach (string separador in separadores.Where(item =>
                !string.IsNullOrEmpty(item) &&
                item is not ".")) {
                preparado = preparado.Replace(
                    separador,
                    " ",
                    StringComparison.Ordinal);
            }

            bool comaEsSeparador = separadores.Contains(
                ",",
                StringComparer.Ordinal);
            string patronNumero = comaEsSeparador
                ? @"(?<![\p{L}\p{N}_])[-+]?(?:\d+(?:\.\d+)?|\.\d+)(?![\p{L}\p{N}_])"
                : @"(?<![\p{L}\p{N}_])[-+]?(?:\d+(?:[.,]\d+)?|[.,]\d+)(?![\p{L}\p{N}_])";
            MatchCollection coincidencias = Regex.Matches(
                preparado,
                patronNumero,
                RegexOptions.CultureInvariant);
            return coincidencias
                .Select(coincidencia => coincidencia.Value)
                .ToArray();
        }

        string patron = string.Join(
            "|",
            separadores
                .Where(item => !string.IsNullOrEmpty(item))
                .Select(Regex.Escape));

        if (string.IsNullOrEmpty(patron)) {
            return string.IsNullOrEmpty(contenido)
                ? Array.Empty<string>()
                : Array.AsReadOnly(new[] { contenido });
        }

        return Regex.Split(contenido, $"(?:{patron})+")
            .Where(elemento => !string.IsNullOrWhiteSpace(elemento))
            .Select(elemento => elemento.Trim())
            .ToArray();
    }

    private static bool EsLineaNumericaEstructurada(
        string linea,
        IReadOnlyList<string> separadores) {
        if (string.IsNullOrWhiteSpace(linea)) {
            return false;
        }

        string restante = Regex.Replace(
            linea,
            @"(?<![\p{L}\p{N}_])[-+]?(?:\d+(?:[.,]\d+)?|[.,]\d+)(?![\p{L}\p{N}_])",
            string.Empty,
            RegexOptions.CultureInvariant);

        foreach (string separador in separadores.Where(item =>
            !string.IsNullOrEmpty(item))) {
            restante = restante.Replace(
                separador,
                string.Empty,
                StringComparison.Ordinal);
        }

        return string.IsNullOrWhiteSpace(restante);
    }

    private static bool CoincideValorEstructurado(
        string valorEncontrado,
        ValorEstructuradoEsperado esperado,
        double? toleranciaNumerica = null,
        bool? distinguirMayusculas = null) {
        return esperado.Tipo switch {
            TipoValorEstructurado.Numerico =>
                ExtraerTokensNumericosEstructurados(valorEncontrado) is
                    { Count: 1 } numeros &&
                IntentarConvertirNumeroEstructurado(numeros[0], out double numero) &&
                SonEquivalentes(
                    numero,
                    esperado.ValorNumerico,
                    toleranciaNumerica ?? esperado.ToleranciaNumerica),
            TipoValorEstructurado.Booleano =>
                IntentarConvertirBooleanoEstructurado(
                    valorEncontrado,
                    esperado,
                    out bool booleano) &&
                booleano == esperado.ValorBooleano,
            TipoValorEstructurado.Textual =>
                ObtenerAlternativasValor(esperado).Any(alternativa =>
                    string.Equals(
                        NormalizarCadenaEstructurada(
                            valorEncontrado,
                            distinguirMayusculas ?? esperado.DistinguirMayusculas,
                            esperado.DistinguirAcentos,
                            esperado.PoliticaEspacios),
                        NormalizarCadenaEstructurada(
                            alternativa,
                            distinguirMayusculas ?? esperado.DistinguirMayusculas,
                            esperado.DistinguirAcentos,
                            esperado.PoliticaEspacios),
                        StringComparison.Ordinal)),
            _ => false
        };
    }

    private static IReadOnlyList<string> ExtraerTokensNumericosEstructurados(
        string texto) {
        return Regex.Matches(
                texto,
                @"(?<![\p{L}\p{N}_])[-+]?(?:\d+(?:[.,]\d+)?|[.,]\d+)(?![\p{L}\p{N}_])",
                RegexOptions.CultureInvariant)
            .Select(coincidencia => coincidencia.Value)
            .ToArray();
    }

    private static IReadOnlyList<string> ObtenerAlternativasValor(
        ValorEstructuradoEsperado esperado) {
        return new[] { esperado.ValorTextual }
            .Concat(esperado.AlternativasTextuales)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static string ExtraerPrimerTokenNumerico(string texto) {
        Match coincidencia = Regex.Match(
            texto,
            @"(?<![\p{L}\p{N}_])[-+]?(?:\d+(?:[.,]\d+)?|[.,]\d+)(?![\p{L}\p{N}_])",
            RegexOptions.CultureInvariant);
        return coincidencia.Success ? coincidencia.Value : texto;
    }

    private static bool IntentarConvertirBooleanoEstructurado(
        string texto,
        ValorEstructuradoEsperado esperado,
        out bool valor) {
        string normalizado = NormalizarBusquedaEstructurada(texto.Trim());
        bool verdadero = esperado.RepresentacionesVerdaderas.Any(item =>
            string.Equals(
                NormalizarBusquedaEstructurada(item),
                normalizado,
                StringComparison.Ordinal));
        bool falso = esperado.RepresentacionesFalsas.Any(item =>
            string.Equals(
                NormalizarBusquedaEstructurada(item),
                normalizado,
                StringComparison.Ordinal));

        if (verdadero == falso) {
            valor = default;
            return false;
        }

        valor = verdadero;
        return true;
    }

    private static string FormatearValorEstructurado(
        ValorEstructuradoEsperado valor) {
        return valor.Tipo switch {
            TipoValorEstructurado.Numerico =>
                valor.ValorNumerico.ToString("G15", CultureInfo.InvariantCulture),
            TipoValorEstructurado.Booleano =>
                valor.ValorBooleano ? "verdadero" : "falso",
            _ => valor.ValorTextual
        };
    }

    private static string FormatearValorEncontrado(
        TipoValorEstructurado tipo,
        string valor) {
        if (tipo == TipoValorEstructurado.Numerico &&
            IntentarConvertirNumeroEstructurado(valor, out double numero)) {
            return numero.ToString("G15", CultureInfo.InvariantCulture);
        }

        return valor;
    }

    private sealed record ResultadoReglasEstructuradas(
        bool TieneReglas,
        bool Coincide,
        bool TieneEstructuraReconocible,
        IReadOnlyList<ResultadoColeccionComparada> Colecciones,
        IReadOnlyList<ResultadoCadenaComparada> Cadenas,
        IReadOnlyList<ResultadoTablaComparada> Tablas,
        IReadOnlyList<ResultadoMatrizComparada> Matrices,
        IReadOnlyList<ResultadoBloquesRegistroComparados> BloquesRegistro,
        IReadOnlyList<string> ReglasCumplidas,
        IReadOnlyList<string> ReglasIncumplidas,
        IReadOnlyList<string> Contradicciones,
        string Mensaje);

    private sealed record LineaEstructurada(
        int Numero,
        int IndiceInicio,
        string Texto);

    private sealed record RegionEstructurada(
        string Etiqueta,
        int LineaInicio,
        IReadOnlyList<LineaEstructurada> Lineas,
        string Contenido);
}
