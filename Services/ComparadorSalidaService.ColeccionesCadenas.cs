using EndForge.Models;

namespace EndForge.Services;

public sealed partial class ComparadorSalidaService {
    private static ResultadoColeccionComparada CompararColeccion(
        string salida,
        ReglaColeccionEsperada regla) {
        int cantidadEsperada = regla.CantidadExacta ??
            regla.ElementosEsperados.Count;
        IReadOnlyList<RegionEstructurada> regiones = ExtraerRegiones(
            salida,
            regla.EtiquetasInicio,
            regla.EtiquetasFin,
            regla.Region,
            regla.RequerirEtiqueta);

        if (regiones.Count == 0) {
            bool coincideAusencia = !regla.Obligatoria;
            return new ResultadoColeccionComparada {
                Nombre = regla.Nombre,
                CantidadEsperada = cantidadEsperada,
                CantidadCorrecta = coincideAusencia && cantidadEsperada == 0,
                OrdenCorrecto = coincideAusencia,
                Coincide = coincideAusencia,
                Mensaje = coincideAusencia
                    ? string.Empty
                    : CrearMensajeColeccion(
                        regla,
                        "No se encontró la etiqueta que delimita la colección.")
            };
        }

        List<IReadOnlyList<string>> elementosPorRegion = regiones
            .Select(region => ExtraerElementosRegionColeccion(region, regla))
            .ToList();
        List<string> encontrados = elementosPorRegion
            .SelectMany(elementos => elementos)
            .ToList();

        if (!regla.Obligatoria &&
            encontrados.Count == 0 &&
            regiones.All(region =>
                string.IsNullOrWhiteSpace(region.Etiqueta))) {
            return new ResultadoColeccionComparada {
                Nombre = regla.Nombre,
                RegionEncontrada = false,
                CantidadEsperada = cantidadEsperada,
                CantidadEncontrada = 0,
                CantidadCorrecta = true,
                OrdenCorrecto = true,
                Coincide = true
            };
        }

        bool tieneContradiccion = elementosPorRegion.Count > 1 &&
            elementosPorRegion
                .Skip(1)
                .Any(elementos => !SonColeccionesEquivalentes(
                    elementosPorRegion[0],
                    elementos,
                    regla));

        bool[] utilizados = new bool[encontrados.Count];
        List<string> faltantes = new();

        foreach (ValorEstructuradoEsperado esperado in regla.ElementosEsperados) {
            int indice = EncontrarElementoColeccion(
                encontrados,
                utilizados,
                esperado,
                regla,
                regla.ConsumirAparicionesUnaVez);

            if (indice < 0) {
                faltantes.Add(FormatearValorEstructurado(esperado));
            } else {
                utilizados[indice] = true;
            }
        }

        List<string> adicionales = utilizados
            .Select((usado, indice) => (usado, indice))
            .Where(item =>
                !item.usado &&
                (!regla.PermitirDuplicados ||
                 !regla.ElementosEsperados.Any(esperado =>
                     CoincideElementoColeccion(
                         encontrados[item.indice],
                         esperado,
                         regla))))
            .Select(item => FormatearValorEncontrado(
                regla.TipoElementos,
                encontrados[item.indice]))
            .ToList();
        List<string> duplicados = regla.PermitirDuplicados
            ? new List<string>()
            : EncontrarDuplicadosColeccion(encontrados, regla);
        bool ordenCorrecto = !regla.OrdenObligatorio ||
            CoincideOrdenColeccion(encontrados, regla);
        bool cantidadCorrecta = regla.CantidadExacta.HasValue
            ? encontrados.Count == regla.CantidadExacta.Value
            : regla.PermitirElementosAdicionales ||
              regla.PermitirDuplicados
                ? encontrados.Count >= regla.ElementosEsperados.Count
                : encontrados.Count == regla.ElementosEsperados.Count;
        bool coincide =
            faltantes.Count == 0 &&
            (regla.PermitirElementosAdicionales || adicionales.Count == 0) &&
            duplicados.Count == 0 &&
            cantidadCorrecta &&
            ordenCorrecto &&
            !tieneContradiccion;

        return new ResultadoColeccionComparada {
            Nombre = regla.Nombre,
            EtiquetaEncontrada = regiones[0].Etiqueta,
            RegionEncontrada = true,
            ElementosEncontrados = encontrados
                .Select(elemento => FormatearValorEncontrado(
                    regla.TipoElementos,
                    elemento))
                .ToArray(),
            ElementosFaltantes = faltantes.AsReadOnly(),
            ElementosAdicionales = adicionales.AsReadOnly(),
            DuplicadosInesperados = duplicados.AsReadOnly(),
            CantidadEsperada = cantidadEsperada,
            CantidadEncontrada = encontrados.Count,
            CantidadCorrecta = cantidadCorrecta,
            OrdenCorrecto = ordenCorrecto,
            TieneContradiccion = tieneContradiccion,
            Coincide = coincide,
            Mensaje = coincide
                ? string.Empty
                : CrearMensajeColeccion(
                    regla,
                    ObtenerDetalleErrorColeccion(
                        faltantes,
                        adicionales,
                        duplicados,
                        cantidadEsperada,
                        encontrados.Count,
                        ordenCorrecto,
                        tieneContradiccion))
        };
    }

    private static IReadOnlyList<string> ExtraerElementosRegionColeccion(
        RegionEstructurada region,
        ReglaColeccionEsperada regla) {
        if (regla.TipoElementos != TipoValorEstructurado.Numerico) {
            return ExtraerElementosColeccion(
                region.Contenido,
                regla.TipoElementos,
                regla.Separadores);
        }

        string contenidoNumerico = string.Join(
            Environment.NewLine,
            region.Lineas
                .Where(linea => EsLineaNumericaEstructurada(
                    linea.Texto,
                    regla.Separadores))
                .Select(linea => linea.Texto));
        return ExtraerElementosColeccion(
            contenidoNumerico,
            regla.TipoElementos,
            regla.Separadores);
    }

    private static int EncontrarElementoColeccion(
        IReadOnlyList<string> encontrados,
        IReadOnlyList<bool> utilizados,
        ValorEstructuradoEsperado esperado,
        ReglaColeccionEsperada regla,
        bool exigirNoUtilizado) {
        for (int indice = 0; indice < encontrados.Count; indice++) {
            if (exigirNoUtilizado && utilizados[indice]) {
                continue;
            }

            if (CoincideElementoColeccion(encontrados[indice], esperado, regla)) {
                return indice;
            }
        }

        return -1;
    }

    private static bool CoincideElementoColeccion(
        string encontrado,
        ValorEstructuradoEsperado esperado,
        ReglaColeccionEsperada regla) {
        if (regla.TipoElementos != esperado.Tipo) {
            return false;
        }

        return CoincideValorEstructurado(
            encontrado,
            esperado,
            regla.ToleranciaNumerica,
            regla.DistinguirMayusculas);
    }

    private static bool CoincideOrdenColeccion(
        IReadOnlyList<string> encontrados,
        ReglaColeccionEsperada regla) {
        if (encontrados.Count < regla.ElementosEsperados.Count) {
            return false;
        }

        if (regla.PermitirDuplicados ||
            regla.PermitirElementosAdicionales) {
            int indiceEncontrado = 0;

            foreach (ValorEstructuradoEsperado esperado in
                     regla.ElementosEsperados) {
                while (indiceEncontrado < encontrados.Count &&
                       !CoincideElementoColeccion(
                           encontrados[indiceEncontrado],
                           esperado,
                           regla)) {
                    indiceEncontrado++;
                }

                if (indiceEncontrado >= encontrados.Count) {
                    return false;
                }

                indiceEncontrado++;
            }

            return true;
        }

        for (int indice = 0;
             indice < regla.ElementosEsperados.Count;
             indice++) {
            if (!CoincideElementoColeccion(
                encontrados[indice],
                regla.ElementosEsperados[indice],
                regla)) {
                return false;
            }
        }

        return regla.PermitirElementosAdicionales ||
            encontrados.Count == regla.ElementosEsperados.Count;
    }

    private static bool SonColeccionesEquivalentes(
        IReadOnlyList<string> izquierda,
        IReadOnlyList<string> derecha,
        ReglaColeccionEsperada regla) {
        if (izquierda.Count != derecha.Count) {
            return false;
        }

        if (regla.OrdenObligatorio) {
            for (int indice = 0; indice < izquierda.Count; indice++) {
                ValorEstructuradoEsperado? esperado = CrearValorDesdeEncontrado(
                    izquierda[indice],
                    regla);

                if (esperado is null ||
                    !CoincideElementoColeccion(derecha[indice], esperado, regla)) {
                    return false;
                }
            }

            return true;
        }

        bool[] usados = new bool[derecha.Count];

        foreach (string valor in izquierda) {
            ValorEstructuradoEsperado? esperado =
                CrearValorDesdeEncontrado(valor, regla);

            if (esperado is null) {
                return false;
            }

            int indice = EncontrarElementoColeccion(
                derecha,
                usados,
                esperado,
                regla,
                true);

            if (indice < 0) {
                return false;
            }

            usados[indice] = true;
        }

        return true;
    }

    private static ValorEstructuradoEsperado? CrearValorDesdeEncontrado(
        string valor,
        ReglaColeccionEsperada regla) {
        if (regla.TipoElementos == TipoValorEstructurado.Numerico) {
            return IntentarConvertirNumeroEstructurado(valor, out double numero)
                ? new ValorEstructuradoEsperado {
                    Tipo = TipoValorEstructurado.Numerico,
                    ValorNumerico = numero,
                    ToleranciaNumerica = regla.ToleranciaNumerica
                }
                : null;
        }

        if (regla.TipoElementos == TipoValorEstructurado.Booleano) {
            ValorEstructuradoEsperado plantilla = regla.ElementosEsperados
                .FirstOrDefault(elemento =>
                    elemento.Tipo == TipoValorEstructurado.Booleano) ??
                new ValorEstructuradoEsperado {
                    Tipo = TipoValorEstructurado.Booleano
                };

            return IntentarConvertirBooleanoEstructurado(
                valor,
                plantilla,
                out bool booleano)
                    ? new ValorEstructuradoEsperado {
                        Tipo = TipoValorEstructurado.Booleano,
                        ValorBooleano = booleano,
                        RepresentacionesVerdaderas =
                            plantilla.RepresentacionesVerdaderas,
                        RepresentacionesFalsas =
                            plantilla.RepresentacionesFalsas
                    }
                    : null;
        }

        return new ValorEstructuradoEsperado {
            Tipo = TipoValorEstructurado.Textual,
            ValorTextual = valor,
            DistinguirMayusculas = regla.DistinguirMayusculas
        };
    }

    private static List<string> EncontrarDuplicadosColeccion(
        IReadOnlyList<string> encontrados,
        ReglaColeccionEsperada regla) {
        List<string> duplicados = new();

        for (int indice = 0; indice < encontrados.Count; indice++) {
            ValorEstructuradoEsperado? esperado =
                CrearValorDesdeEncontrado(encontrados[indice], regla);

            if (esperado is null) {
                continue;
            }

            bool yaAparecio = Enumerable.Range(0, indice).Any(anterior =>
                CoincideElementoColeccion(
                    encontrados[anterior],
                    esperado,
                    regla));

            if (yaAparecio) {
                string representacion = FormatearValorEncontrado(
                    regla.TipoElementos,
                    encontrados[indice]);

                if (!duplicados.Contains(
                    representacion,
                    StringComparer.OrdinalIgnoreCase)) {
                    duplicados.Add(representacion);
                }
            }
        }

        return duplicados;
    }

    private static string ObtenerDetalleErrorColeccion(
        IReadOnlyList<string> faltantes,
        IReadOnlyList<string> adicionales,
        IReadOnlyList<string> duplicados,
        int cantidadEsperada,
        int cantidadEncontrada,
        bool ordenCorrecto,
        bool tieneContradiccion) {
        if (tieneContradiccion) {
            return "La misma etiqueta contiene colecciones contradictorias.";
        }

        if (faltantes.Count > 0) {
            return $"Falta el elemento {faltantes[0]}.";
        }

        if (adicionales.Count > 0) {
            return $"Sobra el elemento {adicionales[0]}.";
        }

        if (duplicados.Count > 0) {
            return $"El elemento {duplicados[0]} está duplicado.";
        }

        if (!ordenCorrecto) {
            return "Los elementos no aparecen en el orden esperado.";
        }

        return $"Se esperaban {cantidadEsperada} elementos y se encontraron {cantidadEncontrada}.";
    }

    private static string CrearMensajeColeccion(
        ReglaColeccionEsperada regla,
        string detalle) {
        return !string.IsNullOrWhiteSpace(regla.MensajeError)
            ? regla.MensajeError
            : $"En la colección {regla.Nombre}, {detalle}";
    }

    private static ResultadoCadenaComparada CompararCadena(
        string salida,
        ReglaCadenaEsperada regla) {
        List<(string Etiqueta, string Valor)> candidatas =
            ObtenerCandidatasCadena(salida, regla);
        bool etiquetaPresente = regla.Origen == OrigenCadenaEsperada.LineaCompleta
            ? candidatas.Count > 0
            : candidatas.Any(candidata =>
                !string.IsNullOrWhiteSpace(candidata.Etiqueta));

        if (candidatas.Count == 0) {
            bool coincideAusencia = !regla.Obligatoria;
            return new ResultadoCadenaComparada {
                Nombre = regla.Nombre,
                ValorEsperado = regla.ValorEsperado,
                EtiquetaPresente = false,
                CoincideMayusculas = coincideAusencia,
                CoincideAcentos = coincideAusencia,
                CoincideEspacios = coincideAusencia,
                Coincide = coincideAusencia,
                Mensaje = coincideAusencia
                    ? string.Empty
                    : CrearMensajeCadena(
                        regla,
                        regla.Origen == OrigenCadenaEsperada.DespuesDeEtiqueta
                            ? "no se encontró una etiqueta reconocida."
                            : "no se encontró una línea para comparar.")
            };
        }

        IReadOnlyList<string> opciones = new[] { regla.ValorEsperado }
            .Concat(regla.AlternativasValidas)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        bool[] coincidencias = candidatas
            .Select(candidata => CoincideCadena(
                candidata.Valor,
                opciones,
                regla,
                regla.PermitirTextoAdicional))
            .ToArray();
        bool coincideAlguna = coincidencias.Any(valor => valor);
        bool tieneContradiccion =
            regla.Origen == OrigenCadenaEsperada.DespuesDeEtiqueta &&
            candidatas.Count > 1 &&
            !coincidencias.All(valor => valor) &&
            candidatas
                .Select(candidata => NormalizarCadenaEstructurada(
                    candidata.Valor,
                    regla.DistinguirMayusculas,
                    regla.DistinguirAcentos,
                    regla.PoliticaEspacios))
                .Distinct(StringComparer.Ordinal)
                .Skip(1)
                .Any();
        bool tieneTextoAdicional =
            regla.Origen == OrigenCadenaEsperada.LineaCompleta &&
            !regla.PermitirTextoAdicional &&
            candidatas.Count != 1;
        string valorDiagnostico = candidatas[0].Valor;
        bool coincideSinDistinguirMayusculas = CoincideCadenaConPoliticas(
            valorDiagnostico,
            opciones,
            regla,
            distinguirMayusculas: false,
            distinguirAcentos: regla.DistinguirAcentos,
            politicaEspacios: regla.PoliticaEspacios);
        bool coincideSinDistinguirAcentos = CoincideCadenaConPoliticas(
            valorDiagnostico,
            opciones,
            regla,
            distinguirMayusculas: regla.DistinguirMayusculas,
            distinguirAcentos: false,
            politicaEspacios: regla.PoliticaEspacios);
        bool coincideConEspaciosFlexibles = CoincideCadenaConPoliticas(
            valorDiagnostico,
            opciones,
            regla,
            distinguirMayusculas: regla.DistinguirMayusculas,
            distinguirAcentos: regla.DistinguirAcentos,
            politicaEspacios: PoliticaEspaciosCadena.ColapsarInternos);
        bool diferenciaMayusculas =
            regla.DistinguirMayusculas &&
            !coincideAlguna &&
            coincideSinDistinguirMayusculas;
        bool diferenciaAcentos =
            regla.DistinguirAcentos &&
            !coincideAlguna &&
            coincideSinDistinguirAcentos;
        bool diferenciaEspacios =
            regla.PoliticaEspacios == PoliticaEspaciosCadena.Exactos &&
            !coincideAlguna &&
            coincideConEspaciosFlexibles;
        bool coincide =
            coincideAlguna &&
            !tieneContradiccion &&
            !tieneTextoAdicional;

        return new ResultadoCadenaComparada {
            Nombre = regla.Nombre,
            ValorEsperado = regla.ValorEsperado,
            ValoresEncontrados = candidatas
                .Select(candidata => candidata.Valor)
                .ToArray(),
            EtiquetaEncontrada = candidatas
                .FirstOrDefault(candidata =>
                    !string.IsNullOrWhiteSpace(candidata.Etiqueta))
                .Etiqueta ?? string.Empty,
            EtiquetaPresente = etiquetaPresente,
            CoincideMayusculas = !diferenciaMayusculas,
            CoincideAcentos = !diferenciaAcentos,
            CoincideEspacios = !diferenciaEspacios,
            TieneTextoAdicional = tieneTextoAdicional,
            TieneContradiccion = tieneContradiccion,
            Coincide = coincide,
            Mensaje = coincide
                ? string.Empty
                : CrearMensajeCadena(
                    regla,
                    ObtenerDetalleErrorCadena(
                        coincideAlguna,
                        diferenciaMayusculas,
                        diferenciaAcentos,
                        diferenciaEspacios,
                        tieneTextoAdicional,
                        tieneContradiccion))
        };
    }

    private static List<(string Etiqueta, string Valor)> ObtenerCandidatasCadena(
        string salida,
        ReglaCadenaEsperada regla) {
        IReadOnlyList<LineaEstructurada> lineas =
            SepararLineasEstructuradas(salida);
        List<(string Etiqueta, string Valor)> candidatas = new();

        if (regla.Origen == OrigenCadenaEsperada.DespuesDeEtiqueta) {
            foreach (LineaEstructurada linea in lineas) {
                if (IntentarExtraerTrasEtiqueta(
                    linea.Texto,
                    regla.EtiquetasAlternativas,
                    out string etiqueta,
                    out string valor,
                    preservarEspaciosValor: true)) {
                    candidatas.Add((etiqueta, valor));
                }
            }

            return candidatas;
        }

        string[] noVacias = lineas
            .Select(linea => linea.Texto)
            .Where(linea => !string.IsNullOrWhiteSpace(linea))
            .ToArray();

        if (noVacias.Length == 0 &&
            string.IsNullOrEmpty(salida) &&
            (string.IsNullOrEmpty(regla.ValorEsperado) ||
             regla.AlternativasValidas.Any(string.IsNullOrEmpty))) {
            candidatas.Add((string.Empty, string.Empty));
        } else {
            candidatas.AddRange(noVacias.Select(linea =>
                (string.Empty, linea)));
        }

        return candidatas;
    }

    private static bool CoincideCadena(
        string valor,
        IReadOnlyList<string> opciones,
        ReglaCadenaEsperada regla,
        bool permitirTextoAdicional) {
        string normalizado = NormalizarCadenaEstructurada(
            valor,
            regla.DistinguirMayusculas,
            regla.DistinguirAcentos,
            regla.PoliticaEspacios);

        return opciones.Any(opcion => {
            string esperada = NormalizarCadenaEstructurada(
                opcion,
                regla.DistinguirMayusculas,
                regla.DistinguirAcentos,
                regla.PoliticaEspacios);

            if (string.IsNullOrEmpty(esperada)) {
                return string.IsNullOrEmpty(normalizado);
            }

            return permitirTextoAdicional
                ? normalizado.Contains(esperada, StringComparison.Ordinal)
                : string.Equals(normalizado, esperada, StringComparison.Ordinal);
        });
    }

    private static bool CoincideCadenaConPoliticas(
        string valor,
        IReadOnlyList<string> opciones,
        ReglaCadenaEsperada regla,
        bool distinguirMayusculas,
        bool distinguirAcentos,
        PoliticaEspaciosCadena politicaEspacios) {
        string normalizado = NormalizarCadenaEstructurada(
            valor,
            distinguirMayusculas,
            distinguirAcentos,
            politicaEspacios);

        return opciones.Any(opcion => {
            string esperada = NormalizarCadenaEstructurada(
                opcion,
                distinguirMayusculas,
                distinguirAcentos,
                politicaEspacios);
            return regla.PermitirTextoAdicional && !string.IsNullOrEmpty(esperada)
                ? normalizado.Contains(esperada, StringComparison.Ordinal)
                : string.Equals(normalizado, esperada, StringComparison.Ordinal);
        });
    }

    private static string ObtenerDetalleErrorCadena(
        bool coincideAlguna,
        bool diferenciaMayusculas,
        bool diferenciaAcentos,
        bool diferenciaEspacios,
        bool tieneTextoAdicional,
        bool tieneContradiccion) {
        if (tieneContradiccion) {
            return "se encontraron valores contradictorios.";
        }

        if (tieneTextoAdicional) {
            return "se encontró texto adicional que la regla no permite.";
        }

        if (diferenciaMayusculas) {
            return "las mayúsculas y minúsculas no coinciden.";
        }

        if (diferenciaAcentos) {
            return "los acentos no coinciden.";
        }

        if (diferenciaEspacios) {
            return "los espacios no coinciden.";
        }

        return coincideAlguna
            ? "la cadena no cumple todas las reglas configuradas."
            : "el valor está ausente, truncado o no coincide.";
    }

    private static string CrearMensajeCadena(
        ReglaCadenaEsperada regla,
        string detalle) {
        return !string.IsNullOrWhiteSpace(regla.MensajeError)
            ? regla.MensajeError
            : $"En la cadena {regla.Nombre}, {detalle}";
    }
}
