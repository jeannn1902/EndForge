using EndForge.Models;
using System.Text.RegularExpressions;

namespace EndForge.Services;

public sealed partial class ComparadorSalidaService {
    private static ResultadoTablaComparada CompararTabla(
        string salida,
        ReglaTablaEsperada regla) {
        int cantidadEsperada = regla.CantidadFilasExacta ??
            regla.FilasEsperadas.Count;
        IReadOnlyList<RegionEstructurada> regiones = ExtraerRegiones(
            salida,
            regla.EtiquetasInicio,
            regla.EtiquetasFin,
            ModoRegionColeccion.BloqueHastaEtiquetaFin,
            regla.EtiquetasInicio.Count > 0);

        if (regiones.Count == 0) {
            bool coincideAusencia = !regla.Obligatoria;
            return new ResultadoTablaComparada {
                Nombre = regla.Nombre,
                CantidadEsperada = cantidadEsperada,
                CantidadCorrecta = coincideAusencia && cantidadEsperada == 0,
                OrdenCorrecto = coincideAusencia,
                Coincide = coincideAusencia,
                Mensaje = coincideAusencia
                    ? string.Empty
                    : CrearMensajeTabla(
                        regla,
                        "no se encontró la región delimitada de la tabla.")
            };
        }

        bool usaClaves = regla.FilasEsperadas.Any(fila =>
            !string.IsNullOrWhiteSpace(fila.Clave) ||
            fila.ClavesAlternativas.Count > 0);
        List<FilaTablaEncontrada> filasEncontradas = new();

        foreach (RegionEstructurada region in regiones) {
            foreach (LineaEstructurada linea in region.Lineas) {
                if (string.IsNullOrWhiteSpace(linea.Texto)) {
                    continue;
                }

                int indiceEsperado = EncontrarIndiceFilaPorClave(
                    linea.Texto,
                    regla.FilasEsperadas,
                    out string clave,
                    out string resto);

                if (indiceEsperado >= 0) {
                    filasEncontradas.Add(new FilaTablaEncontrada(
                        linea.Numero,
                        linea.Texto,
                        clave,
                        resto,
                        indiceEsperado,
                        false));
                    continue;
                }

                bool pareceFila = PareceFilaDeTabla(linea.Texto, regla);

                if (pareceFila ||
                    !regla.PermitirTextoNeutralEntreFilas) {
                    filasEncontradas.Add(new FilaTablaEncontrada(
                        linea.Numero,
                        linea.Texto,
                        string.Empty,
                        ExtraerRestoFilaDesconocida(linea.Texto),
                        null,
                        usaClaves));
                }
            }
        }

        if (!regla.Obligatoria &&
            filasEncontradas.Count == 0 &&
            regiones.All(region =>
                string.IsNullOrWhiteSpace(region.Etiqueta))) {
            return new ResultadoTablaComparada {
                Nombre = regla.Nombre,
                CantidadEsperada = cantidadEsperada,
                CantidadEncontrada = 0,
                CantidadCorrecta = true,
                OrdenCorrecto = true,
                Coincide = true
            };
        }

        bool[] filasUsadas = new bool[filasEncontradas.Count];
        List<ResultadoFilaTablaComparada> resultadosFilas = new();
        List<string> faltantes = new();
        List<string> duplicadas = new();
        List<int> ordenEncontrado = new();
        bool tieneContradiccion = regiones.Count > 1;

        for (int indiceEsperado = 0;
             indiceEsperado < regla.FilasEsperadas.Count;
             indiceEsperado++) {
            FilaTablaEsperada esperada = regla.FilasEsperadas[indiceEsperado];
            List<int> candidatas = EncontrarFilasCandidatas(
                filasEncontradas,
                esperada,
                indiceEsperado,
                usaClaves,
                regla);

            if (candidatas.Count == 0) {
                faltantes.Add(ObtenerNombreFila(esperada, indiceEsperado));
                continue;
            }

            int seleccionada = SeleccionarFilaTabla(
                candidatas,
                filasEncontradas,
                filasUsadas,
                esperada,
                regla);

            if (seleccionada < 0) {
                faltantes.Add(ObtenerNombreFila(esperada, indiceEsperado));
                continue;
            }

            filasUsadas[seleccionada] = true;
            ordenEncontrado.Add(seleccionada);
            ResultadoFilaTablaComparada resultadoFila = CompararFilaTabla(
                filasEncontradas[seleccionada],
                esperada,
                indiceEsperado,
                regla);
            resultadosFilas.Add(resultadoFila);
            tieneContradiccion |= resultadoFila.Celdas.Any(celda =>
                celda.TieneContradiccion);

            int repeticiones = usaClaves
                ? candidatas.Count(indice => indice != seleccionada)
                : 0;

            if (repeticiones > 0) {
                string nombreFila = ObtenerNombreFila(esperada, indiceEsperado);
                duplicadas.Add(nombreFila);
                bool copiasCoherentes = candidatas.All(indice =>
                    CompararFilaTabla(
                        filasEncontradas[indice],
                        esperada,
                        indiceEsperado,
                        regla).Coincide);
                tieneContradiccion |= !copiasCoherentes;

                foreach (int indiceDuplicado in candidatas) {
                    filasUsadas[indiceDuplicado] = true;
                }

                resultadoFila = new ResultadoFilaTablaComparada {
                    Nombre = resultadoFila.Nombre,
                    ClaveEsperada = resultadoFila.ClaveEsperada,
                    ClaveEncontrada = resultadoFila.ClaveEncontrada,
                    NumeroFila = resultadoFila.NumeroFila,
                    Celdas = resultadoFila.Celdas,
                    EsDuplicada = true,
                    Coincide = resultadoFila.Coincide &&
                        regla.PermitirFilasDuplicadas
                };
                resultadosFilas[^1] = resultadoFila;
            }
        }

        List<string> adicionales = filasEncontradas
            .Select((fila, indice) => (fila, indice))
            .Where(item => !filasUsadas[item.indice])
            .Select(item => item.fila.Texto.Trim())
            .ToList();
        bool ordenCorrecto = !regla.OrdenFilasObligatorio ||
            ordenEncontrado.SequenceEqual(
                ordenEncontrado.OrderBy(indice => indice));
        bool cantidadCorrecta = regla.CantidadFilasExacta.HasValue
            ? regla.PermitirFilasAdicionales
                ? filasEncontradas.Count >= cantidadEsperada
                : filasEncontradas.Count == cantidadEsperada
            : regla.PermitirFilasDuplicadas
                ? resultadosFilas.Count == regla.FilasEsperadas.Count
                : regla.PermitirFilasAdicionales
                    ? filasEncontradas.Count >= cantidadEsperada
                    : filasEncontradas.Count == cantidadEsperada;
        int? primeraFilaIncorrecta = resultadosFilas
            .Where(fila => !fila.Coincide)
            .Select(fila => (int?)fila.NumeroFila)
            .FirstOrDefault() ??
            (faltantes.Count > 0 ? resultadosFilas.Count + 1 : null);
        bool coincide =
            faltantes.Count == 0 &&
            (regla.PermitirFilasAdicionales || adicionales.Count == 0) &&
            (regla.PermitirFilasDuplicadas || duplicadas.Count == 0) &&
            resultadosFilas.All(fila => fila.Coincide) &&
            cantidadCorrecta &&
            ordenCorrecto &&
            !tieneContradiccion;

        return new ResultadoTablaComparada {
            Nombre = regla.Nombre,
            Filas = resultadosFilas.AsReadOnly(),
            CantidadEsperada = cantidadEsperada,
            CantidadEncontrada = filasEncontradas.Count,
            CantidadCorrecta = cantidadCorrecta,
            OrdenCorrecto = ordenCorrecto,
            FilasFaltantes = faltantes.AsReadOnly(),
            FilasDuplicadas = duplicadas
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            FilasAdicionales = adicionales.AsReadOnly(),
            PrimeraFilaIncorrecta = primeraFilaIncorrecta,
            TieneContradiccion = tieneContradiccion,
            Coincide = coincide,
            Mensaje = coincide
                ? string.Empty
                : CrearMensajeTabla(
                    regla,
                    ObtenerDetalleErrorTabla(
                        faltantes,
                        adicionales,
                        duplicadas,
                        resultadosFilas,
                        cantidadEsperada,
                        filasEncontradas.Count,
                        ordenCorrecto,
                        tieneContradiccion))
        };
    }

    private static int EncontrarIndiceFilaPorClave(
        string linea,
        IReadOnlyList<FilaTablaEsperada> esperadas,
        out string claveEncontrada,
        out string resto) {
        for (int indice = 0; indice < esperadas.Count; indice++) {
            IReadOnlyList<string> claves = ObtenerClavesFila(esperadas[indice]);

            if (IntentarExtraerTrasEtiqueta(
                linea,
                claves,
                out claveEncontrada,
                out resto)) {
                return indice;
            }
        }

        claveEncontrada = string.Empty;
        resto = linea;
        return -1;
    }

    private static IReadOnlyList<string> ObtenerClavesFila(
        FilaTablaEsperada fila) {
        return new[] { fila.Clave }
            .Concat(fila.ClavesAlternativas)
            .Where(clave => !string.IsNullOrWhiteSpace(clave))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool PareceFilaDeTabla(
        string linea,
        ReglaTablaEsperada regla) {
        IReadOnlyList<string> separadoresNumericos = regla.SeparadoresColumnas
            .Concat(new[] { " " })
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (EsLineaNumericaEstructurada(linea, separadoresNumericos)) {
            return true;
        }

        if (regla.FilasEsperadas
            .SelectMany(fila => fila.Celdas)
            .SelectMany(celda => celda.EtiquetasAlternativas)
            .Any(etiqueta => ContieneEtiquetaEnLinea(linea, etiqueta))) {
            return true;
        }

        string resto = ExtraerRestoFilaDesconocida(linea);

        if (string.Equals(resto, linea.Trim(), StringComparison.Ordinal)) {
            bool tablaSinClaves = regla.FilasEsperadas.All(fila =>
                ObtenerClavesFila(fila).Count == 0);
            return tablaSinClaves &&
                regla.FilasEsperadas.Any(fila =>
                    PareceConjuntoPosicionalDeCeldas(
                        resto,
                        fila.Celdas,
                        regla.SeparadoresColumnas));
        }

        return regla.FilasEsperadas.Any(fila =>
            PareceConjuntoPosicionalDeCeldas(
                resto,
                fila.Celdas,
                regla.SeparadoresColumnas));
    }

    private static bool PareceConjuntoPosicionalDeCeldas(
        string contenido,
        IReadOnlyList<CeldaTablaEsperada> celdas,
        IReadOnlyList<string> separadores) {
        if (celdas.Count == 0 ||
            celdas.Any(celda => celda.EtiquetasAlternativas.Count > 0)) {
            return false;
        }

        IReadOnlyList<CeldaTablaEsperada> ordenadas = celdas
            .OrderBy(celda => celda.Posicion)
            .ToArray();
        IReadOnlyList<string> tokens = ExtraerTokensFilaTabla(
            contenido,
            ordenadas,
            separadores);

        return tokens.Count == ordenadas.Count &&
            ordenadas.All(celda =>
                celda.Posicion >= 0 &&
                celda.Posicion < tokens.Count &&
                EsRepresentacionDelTipo(
                    tokens[celda.Posicion],
                    celda.Valor));
    }

    private static bool EsRepresentacionDelTipo(
        string valor,
        ValorEstructuradoEsperado esperado) {
        return esperado.Tipo switch {
            TipoValorEstructurado.Numerico =>
                ExtraerTokensNumericosEstructurados(valor).Count == 1,
            TipoValorEstructurado.Booleano =>
                IntentarConvertirBooleanoEstructurado(
                    valor,
                    esperado,
                    out _),
            TipoValorEstructurado.Textual =>
                !string.IsNullOrWhiteSpace(valor),
            _ => false
        };
    }

    private static string ExtraerRestoFilaDesconocida(string linea) {
        int separador = linea.IndexOfAny(new[] { ':', '=' });
        return separador >= 0 && separador + 1 < linea.Length
            ? linea[(separador + 1)..].Trim()
            : linea.Trim();
    }

    private static List<int> EncontrarFilasCandidatas(
        IReadOnlyList<FilaTablaEncontrada> encontradas,
        FilaTablaEsperada esperada,
        int indiceEsperado,
        bool usaClaves,
        ReglaTablaEsperada regla) {
        if (!usaClaves && regla.OrdenFilasObligatorio) {
            return indiceEsperado < encontradas.Count
                ? new List<int> { indiceEsperado }
                : new List<int>();
        }

        if (usaClaves && ObtenerClavesFila(esperada).Count > 0) {
            return encontradas
                .Select((fila, indice) => (fila, indice))
                .Where(item => item.fila.IndiceEsperado == indiceEsperado)
                .Select(item => item.indice)
                .ToList();
        }

        return encontradas
            .Select((fila, indice) => (fila, indice))
            .Where(item =>
                !item.fila.EsAdicionalDesconocida &&
                CompararFilaTabla(
                    item.fila,
                    esperada,
                    indiceEsperado,
                    regla).Coincide)
            .Select(item => item.indice)
            .ToList();
    }

    private static int SeleccionarFilaTabla(
        IReadOnlyList<int> candidatas,
        IReadOnlyList<FilaTablaEncontrada> encontradas,
        IReadOnlyList<bool> usadas,
        FilaTablaEsperada esperada,
        ReglaTablaEsperada regla) {
        int primeraDisponible = -1;

        foreach (int indice in candidatas) {
            if (usadas[indice]) {
                continue;
            }

            primeraDisponible = primeraDisponible < 0
                ? indice
                : primeraDisponible;

            if (CompararFilaTabla(
                encontradas[indice],
                esperada,
                indice,
                regla).Coincide) {
                return indice;
            }
        }

        return primeraDisponible;
    }

    private static ResultadoFilaTablaComparada CompararFilaTabla(
        FilaTablaEncontrada encontrada,
        FilaTablaEsperada esperada,
        int indiceEsperado,
        ReglaTablaEsperada regla) {
        IReadOnlyList<CeldaTablaEsperada> celdasEsperadas = esperada.Celdas
            .OrderBy(celda => celda.Posicion)
            .ToArray();
        IReadOnlyList<string> tokensPosicionales = ExtraerTokensFilaTabla(
            encontrada.Resto,
            celdasEsperadas,
            regla.SeparadoresColumnas);
        List<ResultadoCeldaTablaComparada> celdas = new();
        HashSet<int> columnasEncontradas = new();

        foreach (CeldaTablaEsperada celda in celdasEsperadas) {
            IReadOnlyList<string> valores = celda.EtiquetasAlternativas.Count > 0
                ? ExtraerValoresEtiquetadosEnLinea(
                    encontrada.Texto,
                    celda.EtiquetasAlternativas,
                    celdasEsperadas.SelectMany(item =>
                        item.EtiquetasAlternativas))
                : celda.Posicion >= 0 && celda.Posicion < tokensPosicionales.Count
                    ? Array.AsReadOnly(new[] {
                        tokensPosicionales[celda.Posicion]
                    })
                    : Array.Empty<string>();
            bool tieneContradiccion = valores.Count > 1 &&
                valores
                    .Select(valor => CoincideValorEstructurado(
                        valor,
                        celda.Valor))
                    .Distinct()
                    .Count() > 1;
            bool coincide = valores.Count > 0 &&
                valores.All(valor => CoincideValorEstructurado(
                    valor,
                    celda.Valor)) &&
                !tieneContradiccion;

            if (valores.Count > 0) {
                columnasEncontradas.Add(celda.Posicion);
            }

            celdas.Add(new ResultadoCeldaTablaComparada {
                Nombre = celda.Nombre,
                Fila = encontrada.NumeroLinea + 1,
                Columna = celda.Posicion + 1,
                ValorEsperado = FormatearValorEstructurado(celda.Valor),
                ValorEncontrado = valores.FirstOrDefault() ?? string.Empty,
                TieneContradiccion = tieneContradiccion,
                Coincide = coincide
            });
        }

        int columnasEsperadas = regla.CantidadColumnasExacta ??
            celdasEsperadas.Count;
        bool usaSoloPosiciones = celdasEsperadas.All(celda =>
            celda.EtiquetasAlternativas.Count == 0);
        int cantidadColumnasEncontradas = usaSoloPosiciones
            ? tokensPosicionales.Count
            : columnasEncontradas.Count;
        bool cantidadColumnasCorrecta =
            celdasEsperadas.Count == columnasEsperadas &&
            cantidadColumnasEncontradas == columnasEsperadas;

        if (!cantidadColumnasCorrecta) {
            celdas.Add(new ResultadoCeldaTablaComparada {
                Nombre = cantidadColumnasEncontradas > columnasEsperadas
                    ? "Celda adicional"
                    : "Celda faltante",
                Fila = encontrada.NumeroLinea + 1,
                Columna = Math.Min(
                    cantidadColumnasEncontradas + 1,
                    columnasEsperadas + 1),
                ValorEsperado = columnasEsperadas.ToString(
                    System.Globalization.CultureInfo.InvariantCulture),
                ValorEncontrado = cantidadColumnasEncontradas.ToString(
                    System.Globalization.CultureInfo.InvariantCulture),
                Coincide = false
            });
        }

        IReadOnlyList<string> etiquetasConocidas = ObtenerClavesFila(esperada)
            .Concat(celdasEsperadas.SelectMany(celda =>
                celda.EtiquetasAlternativas))
            .ToArray();
        string? etiquetaDesconocida = EncontrarEtiquetasEnLinea(encontrada.Texto)
            .FirstOrDefault(etiqueta => !etiquetasConocidas.Any(conocida =>
                string.Equals(
                    NormalizarBusquedaEstructurada(conocida),
                    NormalizarBusquedaEstructurada(etiqueta),
                    StringComparison.Ordinal)));

        if (!string.IsNullOrWhiteSpace(etiquetaDesconocida)) {
            celdas.Add(new ResultadoCeldaTablaComparada {
                Nombre = "Etiqueta de celda desconocida",
                Fila = encontrada.NumeroLinea + 1,
                Columna = columnasEsperadas + 1,
                ValorEncontrado = etiquetaDesconocida,
                Coincide = false
            });
        }

        return new ResultadoFilaTablaComparada {
            Nombre = ObtenerNombreFila(esperada, indiceEsperado),
            ClaveEsperada = esperada.Clave,
            ClaveEncontrada = encontrada.Clave,
            NumeroFila = encontrada.NumeroLinea + 1,
            Celdas = celdas.AsReadOnly(),
            Coincide = celdas.Count == celdasEsperadas.Count &&
                celdas.All(celda => celda.Coincide)
        };
    }

    private static IReadOnlyList<string> EncontrarEtiquetasEnLinea(string linea) {
        return Regex.Matches(
                linea,
                @"(?:^|[|;,])\s*(?<etiqueta>[\p{L}][\p{L}\p{N}_]*(?:[ \t]+[\p{L}\p{N}_]+){0,4})[ \t]*[:=]",
                RegexOptions.CultureInvariant)
            .Select(coincidencia =>
                coincidencia.Groups["etiqueta"].Value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<string> ExtraerTokensFilaTabla(
        string contenido,
        IReadOnlyList<CeldaTablaEsperada> celdas,
        IReadOnlyList<string> separadores) {
        if (celdas.Count == 1 &&
            celdas[0].Valor.Tipo == TipoValorEstructurado.Textual) {
            return Array.AsReadOnly(new[] { contenido.Trim() });
        }

        List<string> separadoresEfectivos = separadores
            .Where(item => !string.IsNullOrEmpty(item))
            .ToList();

        if (celdas.All(celda =>
            celda.Valor.Tipo != TipoValorEstructurado.Textual)) {
            separadoresEfectivos.Add(" ");
        }

        string patron = string.Join(
            "|",
            separadoresEfectivos
                .Distinct(StringComparer.Ordinal)
                .Select(Regex.Escape));

        return string.IsNullOrEmpty(patron)
            ? Array.AsReadOnly(new[] { contenido.Trim() })
            : Regex.Split(contenido.Trim(), $"(?:{patron})+")
                .Where(valor => !string.IsNullOrWhiteSpace(valor))
                .Select(valor => valor.Trim())
                .ToArray();
    }

    private static IReadOnlyList<string> ExtraerValoresEtiquetadosEnLinea(
        string linea,
        IReadOnlyList<string> etiquetasObjetivo,
        IEnumerable<string> todasLasEtiquetas) {
        List<(int Indice, int Longitud, string Etiqueta)> apariciones =
            todasLasEtiquetas
                .Where(etiqueta => !string.IsNullOrWhiteSpace(etiqueta))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .SelectMany(etiqueta => EncontrarAparicionesEtiqueta(
                    linea,
                    etiqueta))
                .OrderBy(item => item.Indice)
                .ToList();
        List<string> valores = new();

        foreach ((int indice, int longitud, string etiqueta) in apariciones) {
            if (!etiquetasObjetivo.Contains(
                etiqueta,
                StringComparer.OrdinalIgnoreCase)) {
                continue;
            }

            int inicio = indice + longitud;

            while (inicio < linea.Length &&
                   (char.IsWhiteSpace(linea[inicio]) ||
                    linea[inicio] is ':' or '=' or '-')) {
                inicio++;
            }

            int fin = apariciones
                .Where(item => item.Indice > indice)
                .Select(item => item.Indice)
                .DefaultIfEmpty(linea.Length)
                .Min();
            valores.Add(linea[inicio..fin]
                .Trim()
                .TrimEnd(',', ';', '|'));
        }

        return valores.AsReadOnly();
    }

    private static IEnumerable<(int Indice, int Longitud, string Etiqueta)>
        EncontrarAparicionesEtiqueta(
            string linea,
            string etiqueta) {
        int inicioBusqueda = 0;

        while (inicioBusqueda < linea.Length) {
            int indice = linea.IndexOf(
                etiqueta,
                inicioBusqueda,
                StringComparison.OrdinalIgnoreCase);

            if (indice < 0) {
                yield break;
            }

            bool limiteInicial = indice == 0 ||
                !char.IsLetterOrDigit(linea[indice - 1]);
            int fin = indice + etiqueta.Length;
            bool limiteFinal = fin >= linea.Length ||
                !char.IsLetterOrDigit(linea[fin]);

            if (limiteInicial && limiteFinal) {
                yield return (indice, etiqueta.Length, etiqueta);
            }

            inicioBusqueda = indice + Math.Max(1, etiqueta.Length);
        }
    }

    private static bool ContieneEtiquetaEnLinea(
        string linea,
        string etiqueta) {
        return EncontrarAparicionesEtiqueta(linea, etiqueta).Any();
    }

    private static string ObtenerNombreFila(
        FilaTablaEsperada fila,
        int indice) {
        return !string.IsNullOrWhiteSpace(fila.Nombre)
            ? fila.Nombre
            : !string.IsNullOrWhiteSpace(fila.Clave)
                ? fila.Clave
                : $"Fila {indice + 1}";
    }

    private static string ObtenerDetalleErrorTabla(
        IReadOnlyList<string> faltantes,
        IReadOnlyList<string> adicionales,
        IReadOnlyList<string> duplicadas,
        IReadOnlyList<ResultadoFilaTablaComparada> filas,
        int cantidadEsperada,
        int cantidadEncontrada,
        bool ordenCorrecto,
        bool tieneContradiccion) {
        if (tieneContradiccion) {
            return "una fila o celda contiene valores contradictorios.";
        }

        if (faltantes.Count > 0) {
            return $"falta la fila {faltantes[0]}.";
        }

        if (adicionales.Count > 0) {
            return $"se encontró una fila adicional: {adicionales[0]}.";
        }

        if (duplicadas.Count > 0) {
            return $"la fila {duplicadas[0]} está duplicada.";
        }

        if (!ordenCorrecto) {
            return "las filas no aparecen en el orden esperado.";
        }

        ResultadoCeldaTablaComparada? celda = filas
            .SelectMany(fila => fila.Celdas)
            .FirstOrDefault(item => !item.Coincide);

        if (celda is not null) {
            return $"la celda de la fila {celda.Fila}, columna {celda.Columna} no coincide.";
        }

        return $"se esperaban {cantidadEsperada} filas y se encontraron {cantidadEncontrada}.";
    }

    private static string CrearMensajeTabla(
        ReglaTablaEsperada regla,
        string detalle) {
        return !string.IsNullOrWhiteSpace(regla.MensajeError)
            ? regla.MensajeError
            : $"En la tabla {regla.Nombre}, {detalle}";
    }

    private static ResultadoMatrizComparada CompararMatriz(
        string salida,
        ReglaMatrizEsperada regla) {
        IReadOnlyList<RegionEstructurada> regiones = ExtraerRegiones(
            salida,
            regla.EtiquetasInicio,
            Array.Empty<string>(),
            ModoRegionColeccion.BloqueHastaEtiquetaFin,
            regla.RequerirEtiqueta);

        if (regiones.Count == 0) {
            bool coincideAusencia = !regla.Obligatoria;
            return new ResultadoMatrizComparada {
                Nombre = regla.Nombre,
                FilasEsperadas = regla.FilasEsperadas,
                ColumnasEsperadas = regla.ColumnasEsperadas,
                DimensionesCorrectas = coincideAusencia &&
                    regla.FilasEsperadas == 0 &&
                    regla.ColumnasEsperadas == 0,
                Coincide = coincideAusencia,
                Mensaje = coincideAusencia
                    ? string.Empty
                    : CrearMensajeMatriz(
                        regla,
                        "no se encontró la matriz esperada.")
            };
        }

        List<IReadOnlyList<string>> filas = ExtraerFilasMatriz(
            regiones[0],
            regla);

        if (!regla.Obligatoria &&
            filas.Count == 0 &&
            string.IsNullOrWhiteSpace(regiones[0].Etiqueta)) {
            return new ResultadoMatrizComparada {
                Nombre = regla.Nombre,
                FilasEsperadas = regla.FilasEsperadas,
                ColumnasEsperadas = regla.ColumnasEsperadas,
                FilasEncontradas = 0,
                ColumnasEncontradas = 0,
                DimensionesCorrectas = true,
                Coincide = true
            };
        }

        int filasEncontradas = filas.Count;
        int columnasEncontradas = filas.Count == 0
            ? 0
            : filas.Max(fila => fila.Count);
        bool dimensionesCorrectas = regla.PermitirElementosAdicionales
            ? filasEncontradas >= regla.FilasEsperadas &&
              filas.Take(regla.FilasEsperadas).All(fila =>
                  fila.Count >= regla.ColumnasEsperadas)
            : filasEncontradas == regla.FilasEsperadas &&
              filas.All(fila => fila.Count == regla.ColumnasEsperadas);
        List<string> filasIncompletas = filas
            .Select((fila, indice) => (fila, indice))
            .Where(item => item.fila.Count < regla.ColumnasEsperadas)
            .Select(item => $"Fila {item.indice + 1}")
            .ToList();
        List<string> adicionales = ObtenerElementosAdicionalesMatriz(
            filas,
            regla);
        string primeraCeldaIncorrecta = EncontrarPrimeraCeldaIncorrecta(
            filas,
            regla);
        bool valoresCorrectos = string.IsNullOrEmpty(primeraCeldaIncorrecta);
        bool esTranspuesta = EsMatrizTranspuesta(filas, regla);
        bool coincide =
            dimensionesCorrectas &&
            valoresCorrectos &&
            (regla.PermitirElementosAdicionales || adicionales.Count == 0) &&
            regiones.Count == 1;

        return new ResultadoMatrizComparada {
            Nombre = regla.Nombre,
            FilasEsperadas = regla.FilasEsperadas,
            ColumnasEsperadas = regla.ColumnasEsperadas,
            FilasEncontradas = filasEncontradas,
            ColumnasEncontradas = columnasEncontradas,
            DimensionesCorrectas = dimensionesCorrectas,
            EsTranspuesta = esTranspuesta,
            PrimeraCeldaIncorrecta = primeraCeldaIncorrecta,
            ElementosAdicionales = adicionales.AsReadOnly(),
            FilasIncompletas = filasIncompletas.AsReadOnly(),
            TieneContradiccion = regiones.Count > 1,
            Coincide = coincide,
            Mensaje = coincide
                ? string.Empty
                : CrearMensajeMatriz(
                    regla,
                    ObtenerDetalleErrorMatriz(
                        regla,
                        filasEncontradas,
                        columnasEncontradas,
                        filasIncompletas,
                        adicionales,
                        primeraCeldaIncorrecta,
                        esTranspuesta,
                        regiones.Count > 1))
        };
    }

    private static List<IReadOnlyList<string>> ExtraerFilasMatriz(
        RegionEstructurada region,
        ReglaMatrizEsperada regla) {
        List<IReadOnlyList<string>> filas = new();
        List<IReadOnlyList<string>> filasNumericasPendientes = new();
        bool comenzoMatriz =
            regla.RequerirEtiqueta ||
            !string.IsNullOrWhiteSpace(region.Etiqueta);

        foreach (LineaEstructurada linea in region.Lineas) {
            IReadOnlyList<string> elementos = ExtraerElementosColeccion(
                linea.Texto,
                regla.TipoElementos,
                regla.SeparadoresColumnas);
            bool esFila = elementos.Count > 0 &&
                (regla.TipoElementos == TipoValorEstructurado.Numerico
                    ? EsLineaNumericaEstructurada(
                        linea.Texto,
                        regla.SeparadoresColumnas)
                    : regla.TipoElementos == TipoValorEstructurado.Textual);
            bool cantidadCompatibleParaInicio =
                elementos.Count >= regla.ColumnasEsperadas ||
                elementos.Count == regla.FilasEsperadas;
            bool puedeIniciar =
                !regla.PermitirTextoNeutralExterno ||
                cantidadCompatibleParaInicio &&
                (regla.TipoElementos != TipoValorEstructurado.Textual ||
                 ContieneValorTextualEsperado(elementos, regla));

            if (comenzoMatriz &&
                filas.Count >= regla.FilasEsperadas &&
                regla.PermitirTextoNeutralExterno &&
                regla.TipoElementos == TipoValorEstructurado.Textual &&
                !ContieneValorTextualEsperado(elementos, regla)) {
                break;
            }

            if (esFila &&
                (comenzoMatriz || puedeIniciar)) {
                if (!comenzoMatriz &&
                    filasNumericasPendientes.Count > 0) {
                    filas.AddRange(filasNumericasPendientes);
                    filasNumericasPendientes.Clear();
                }

                comenzoMatriz = true;
                filas.Add(elementos);
            } else if (!comenzoMatriz &&
                       esFila &&
                       regla.TipoElementos ==
                           TipoValorEstructurado.Numerico) {
                filasNumericasPendientes.Add(elementos);
            } else if (!string.IsNullOrWhiteSpace(linea.Texto) &&
                       !regla.PermitirTextoNeutralExterno) {
                filas.Add(Array.AsReadOnly(new[] { linea.Texto.Trim() }));
            } else if (comenzoMatriz &&
                       filas.Count < regla.FilasEsperadas &&
                       !string.IsNullOrWhiteSpace(linea.Texto)) {
                filas.Add(Array.AsReadOnly(new[] { linea.Texto.Trim() }));
            } else if (comenzoMatriz &&
                       filas.Count >= regla.FilasEsperadas &&
                       !string.IsNullOrWhiteSpace(linea.Texto)) {
                break;
            } else if (!comenzoMatriz) {
                filasNumericasPendientes.Clear();
            }
        }

        return filas;
    }

    private static bool ContieneValorTextualEsperado(
        IReadOnlyList<string> elementos,
        ReglaMatrizEsperada regla) {
        string[] esperados = regla.ValoresTextualesEsperados
            .SelectMany(fila => fila)
            .ToArray();

        return elementos.Any(elemento => esperados.Any(esperado =>
            string.Equals(
                NormalizarCadenaEstructurada(
                    elemento,
                    regla.DistinguirMayusculas,
                    regla.DistinguirAcentos,
                    regla.PoliticaEspacios),
                NormalizarCadenaEstructurada(
                    esperado,
                    regla.DistinguirMayusculas,
                    regla.DistinguirAcentos,
                    regla.PoliticaEspacios),
                StringComparison.Ordinal)));
    }

    private static List<string> ObtenerElementosAdicionalesMatriz(
        IReadOnlyList<IReadOnlyList<string>> filas,
        ReglaMatrizEsperada regla) {
        List<string> adicionales = new();

        for (int fila = 0; fila < filas.Count; fila++) {
            if (fila >= regla.FilasEsperadas) {
                adicionales.Add($"Fila {fila + 1}");
                continue;
            }

            for (int columna = regla.ColumnasEsperadas;
                 columna < filas[fila].Count;
                 columna++) {
                adicionales.Add($"[{fila + 1},{columna + 1}]");
            }
        }

        return adicionales;
    }

    private static string EncontrarPrimeraCeldaIncorrecta(
        IReadOnlyList<IReadOnlyList<string>> filas,
        ReglaMatrizEsperada regla) {
        int limiteFilas = Math.Min(regla.FilasEsperadas, filas.Count);

        for (int fila = 0; fila < limiteFilas; fila++) {
            int limiteColumnas = Math.Min(
                regla.ColumnasEsperadas,
                filas[fila].Count);

            for (int columna = 0; columna < limiteColumnas; columna++) {
                if (!CoincideCeldaMatriz(
                    filas[fila][columna],
                    fila,
                    columna,
                    regla)) {
                    return $"[{fila + 1},{columna + 1}]";
                }
            }
        }

        return string.Empty;
    }

    private static bool CoincideCeldaMatriz(
        string encontrada,
        int fila,
        int columna,
        ReglaMatrizEsperada regla) {
        if (regla.TipoElementos == TipoValorEstructurado.Numerico) {
            return fila < regla.ValoresNumericosEsperados.Count &&
                columna < regla.ValoresNumericosEsperados[fila].Count &&
                IntentarConvertirNumeroEstructurado(encontrada, out double valor) &&
                SonEquivalentes(
                    valor,
                    regla.ValoresNumericosEsperados[fila][columna],
                    regla.ToleranciaNumerica);
        }

        if (regla.TipoElementos == TipoValorEstructurado.Textual) {
            return fila < regla.ValoresTextualesEsperados.Count &&
                columna < regla.ValoresTextualesEsperados[fila].Count &&
                string.Equals(
                    NormalizarCadenaEstructurada(
                        encontrada,
                        regla.DistinguirMayusculas,
                        regla.DistinguirAcentos,
                        regla.PoliticaEspacios),
                    NormalizarCadenaEstructurada(
                        regla.ValoresTextualesEsperados[fila][columna],
                        regla.DistinguirMayusculas,
                        regla.DistinguirAcentos,
                        regla.PoliticaEspacios),
                    StringComparison.Ordinal);
        }

        return false;
    }

    private static bool EsMatrizTranspuesta(
        IReadOnlyList<IReadOnlyList<string>> filas,
        ReglaMatrizEsperada regla) {
        if (filas.Count != regla.ColumnasEsperadas ||
            filas.Any(fila => fila.Count != regla.FilasEsperadas)) {
            return false;
        }

        for (int fila = 0; fila < filas.Count; fila++) {
            for (int columna = 0; columna < filas[fila].Count; columna++) {
                if (!CoincideCeldaMatriz(
                    filas[fila][columna],
                    columna,
                    fila,
                    regla)) {
                    return false;
                }
            }
        }

        return regla.FilasEsperadas != regla.ColumnasEsperadas ||
            !string.IsNullOrEmpty(EncontrarPrimeraCeldaIncorrecta(filas, regla));
    }

    private static string ObtenerDetalleErrorMatriz(
        ReglaMatrizEsperada regla,
        int filasEncontradas,
        int columnasEncontradas,
        IReadOnlyList<string> filasIncompletas,
        IReadOnlyList<string> adicionales,
        string primeraCeldaIncorrecta,
        bool esTranspuesta,
        bool regionesMultiples) {
        if (regionesMultiples) {
            return "se encontraron varias regiones contradictorias.";
        }

        if (esTranspuesta) {
            return "las filas y columnas aparecen transpuestas.";
        }

        if (filasIncompletas.Count > 0) {
            return $"{filasIncompletas[0]} está incompleta.";
        }

        if (adicionales.Count > 0) {
            return $"sobra el elemento {adicionales[0]}.";
        }

        if (filasEncontradas != regla.FilasEsperadas ||
            columnasEncontradas != regla.ColumnasEsperadas) {
            return $"se esperaba una matriz de {regla.FilasEsperadas}×{regla.ColumnasEsperadas} y se encontró una de {filasEncontradas}×{columnasEncontradas}.";
        }

        if (!string.IsNullOrEmpty(primeraCeldaIncorrecta)) {
            return $"la celda {primeraCeldaIncorrecta} no coincide.";
        }

        return "la forma o los valores no coinciden.";
    }

    private static string CrearMensajeMatriz(
        ReglaMatrizEsperada regla,
        string detalle) {
        return !string.IsNullOrWhiteSpace(regla.MensajeError)
            ? regla.MensajeError
            : $"En la matriz {regla.Nombre}, {detalle}";
    }

    private sealed record FilaTablaEncontrada(
        int NumeroLinea,
        string Texto,
        string Clave,
        string Resto,
        int? IndiceEsperado,
        bool EsAdicionalDesconocida);
}
