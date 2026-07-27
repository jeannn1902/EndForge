using System.Text.Json;
using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class CatalogoEvaluacionesTests {
    [Fact]
    public void GradoDos_TodasLasDefinicionesCumplenLaPoliticaPublicada() {
        DefinicionEvaluacionPractica[] definiciones =
            CatalogoTestHelper.CargarDefiniciones(
                CatalogoTestHelper.IdsPracticasGradoDos);

        Assert.Equal(40, definiciones.Length);

        Assert.All(definiciones, definicion => {
            Assert.Equal(5, definicion.CasosPrueba.Count);
            Assert.All(
                definicion.CasosPrueba,
                caso => Assert.Equal(12, caso.Puntos));
            Assert.Equal(60, definicion.PuntosCasosPrueba);
            Assert.Contains(
                definicion.CasosPrueba,
                caso => !caso.EsVisible);
            Assert.Equal(100, definicion.PuntosMaximos);
            AssertDefinicionValida(definicion);
        });
    }

    [Fact]
    public void GradoUno_ConservaSusDefinicionesLegadasSinForzarCincoCasos() {
        DefinicionEvaluacionPractica[] definiciones =
            CatalogoTestHelper.CargarDefiniciones(
                CatalogoTestHelper.IdsPracticasGradoUno);

        Assert.Equal(20, definiciones.Length);
        Assert.Empty(
            CatalogoTestHelper.IdsPracticasGradoUno.Except(
                definiciones.Select(definicion => definicion.PracticaId),
                StringComparer.OrdinalIgnoreCase));
        Assert.Empty(
            definiciones.Select(definicion => definicion.PracticaId).Except(
                CatalogoTestHelper.IdsPracticasGradoUno,
                StringComparer.OrdinalIgnoreCase));

        Assert.All(definiciones, definicion => {
            Assert.NotEmpty(definicion.CasosPrueba);
            Assert.Equal(60, definicion.PuntosCasosPrueba);
            Assert.Equal(100, definicion.PuntosMaximos);
            AssertDefinicionValida(definicion);
        });
    }

    [Fact]
    public void GradoDos_LasEntradasYReglasSonDeterministasEntreInstancias() {
        DefinicionEvaluacionPractica[] primeraCarga =
            CatalogoTestHelper.CargarDefiniciones(
                CatalogoTestHelper.IdsPracticasGradoDos);
        DefinicionEvaluacionPractica[] segundaCarga =
            CatalogoTestHelper.CargarDefiniciones(
                CatalogoTestHelper.IdsPracticasGradoDos);

        string primeraRepresentacion =
            JsonSerializer.Serialize(primeraCarga);
        string segundaRepresentacion =
            JsonSerializer.Serialize(segundaCarga);

        Assert.Equal(primeraRepresentacion, segundaRepresentacion);
    }

    private static void AssertDefinicionValida(
        DefinicionEvaluacionPractica definicion) {
        Assert.False(string.IsNullOrWhiteSpace(definicion.PracticaId));
        Assert.False(string.IsNullOrWhiteSpace(definicion.NombrePractica));
        Assert.NotEmpty(definicion.Criterios);
        Assert.Equal(
            definicion.Criterios.Count,
            definicion.Criterios
                .Select(criterio => criterio.Id)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count());
        Assert.All(definicion.Criterios, criterio => {
            Assert.False(string.IsNullOrWhiteSpace(criterio.Id));
            Assert.False(string.IsNullOrWhiteSpace(criterio.Nombre));
            Assert.True(criterio.PuntosMaximos > 0);
        });

        Assert.Equal(
            definicion.CasosPrueba.Count,
            definicion.CasosPrueba
                .Select(caso => caso.Id)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count());

        Assert.All(definicion.CasosPrueba, caso => {
            Assert.False(string.IsNullOrWhiteSpace(caso.Id));
            Assert.True(caso.Puntos > 0);
            Assert.True(TieneReglaAplicable(caso));
            AssertEtiquetasYReglasNombradas(caso);
            AssertToleranciasValidas(caso);
            AssertCantidadesValidas(caso);
            AssertRutasArchivosSeguras(caso);
        });
    }

    private static bool TieneReglaAplicable(CasoPrueba caso) {
        return caso.TokensObligatorios.Count > 0 ||
            caso.GruposTokensAlternativos.Count > 0 ||
            caso.ValoresNumericosEsperados.Count > 0 ||
            caso.ValoresBooleanosEsperados.Count > 0 ||
            caso.ValoresTextualesEsperados.Count > 0 ||
            caso.SecuenciasEsperadas.Count > 0 ||
            caso.SecuenciasCompuestasEsperadas.Count > 0 ||
            caso.ColeccionesEsperadas.Count > 0 ||
            caso.CadenasEsperadas.Count > 0 ||
            caso.TablasEsperadas.Count > 0 ||
            caso.MatricesEsperadas.Count > 0 ||
            caso.BloquesRegistroEsperados.Count > 0 ||
            caso.SalidaExactaEsperada is not null ||
            caso.ArchivosEsperados.Count > 0;
    }

    private static void AssertEtiquetasYReglasNombradas(CasoPrueba caso) {
        List<string> nombres = new();
        List<string> etiquetas = new();
        List<string> separadores = new();

        nombres.AddRange(caso.GruposTokensAlternativos.Select(grupo => grupo.Nombre));
        etiquetas.AddRange(caso.TokensObligatorios);
        etiquetas.AddRange(caso.GruposTokensAlternativos.SelectMany(
            grupo => grupo.Alternativas.Concat(grupo.EtiquetasAsociadas)));

        nombres.AddRange(caso.ValoresNumericosEsperados.Select(valor => valor.Nombre));
        etiquetas.AddRange(caso.ValoresNumericosEsperados.SelectMany(
            valor => valor.EtiquetasAlternativas));

        nombres.AddRange(caso.ValoresBooleanosEsperados.Select(valor => valor.Nombre));
        etiquetas.AddRange(caso.ValoresBooleanosEsperados.SelectMany(
            valor => valor.EtiquetasAlternativas
                .Concat(valor.RepresentacionesVerdaderas)
                .Concat(valor.RepresentacionesFalsas)));

        nombres.AddRange(caso.ValoresTextualesEsperados.Select(valor => valor.Nombre));
        etiquetas.AddRange(caso.ValoresTextualesEsperados.SelectMany(
            valor => valor.EtiquetasAlternativas
                .Concat(valor.Opciones.Select(opcion => opcion.Valor))
                .Concat(valor.Opciones.SelectMany(opcion => opcion.Alternativas))));

        foreach (SecuenciaEsperada secuencia in caso.SecuenciasEsperadas) {
            nombres.Add(secuencia.Nombre);
            separadores.AddRange(secuencia.SeparadoresPermitidos);
            AgregarEventos(
                secuencia.AlternativasTextualesEsperadas,
                nombres,
                etiquetas);
            AgregarEventos(
                secuencia.EventosTextualesReconocibles,
                nombres,
                etiquetas);
        }

        foreach (SecuenciaCompuestaEsperada secuencia
            in caso.SecuenciasCompuestasEsperadas) {
            nombres.Add(secuencia.Nombre);
            etiquetas.AddRange(secuencia.SeparadoresTextualesPermitidos);

            foreach (PasoSecuenciaCompuestaEsperado paso
                in secuencia.PasosEsperados) {
                nombres.Add(paso.Nombre);
                nombres.AddRange(paso.Componentes.Select(
                    componente => componente.Nombre));
                separadores.AddRange(paso.Componentes.SelectMany(
                    componente =>
                        componente.EtiquetasOSeparadoresOpcionales));
            }

            separadores.AddRange(
                secuencia.SeparadoresTextualesPermitidos);
        }

        foreach (ReglaColeccionEsperada regla in caso.ColeccionesEsperadas) {
            nombres.Add(regla.Nombre);
            nombres.AddRange(regla.ElementosEsperados.Select(
                elemento => elemento.Nombre));
            etiquetas.AddRange(regla.EtiquetasInicio);
            etiquetas.AddRange(regla.EtiquetasFin);
            separadores.AddRange(regla.Separadores);
            etiquetas.AddRange(regla.ElementosEsperados.SelectMany(
                elemento => elemento.AlternativasTextuales
                    .Concat(elemento.RepresentacionesVerdaderas)
                    .Concat(elemento.RepresentacionesFalsas)));
        }

        foreach (ReglaCadenaEsperada regla in caso.CadenasEsperadas) {
            nombres.Add(regla.Nombre);
            etiquetas.AddRange(regla.EtiquetasAlternativas);
            etiquetas.AddRange(regla.AlternativasValidas);
        }

        foreach (ReglaTablaEsperada regla in caso.TablasEsperadas) {
            nombres.Add(regla.Nombre);
            etiquetas.AddRange(regla.EtiquetasInicio);
            etiquetas.AddRange(regla.EtiquetasFin);
            separadores.AddRange(regla.SeparadoresColumnas);

            foreach (FilaTablaEsperada fila in regla.FilasEsperadas) {
                nombres.Add(fila.Nombre);
                etiquetas.Add(fila.Clave);
                etiquetas.AddRange(fila.ClavesAlternativas);

                foreach (CeldaTablaEsperada celda in fila.Celdas) {
                    nombres.Add(celda.Nombre);
                    nombres.Add(celda.Valor.Nombre);
                    etiquetas.AddRange(celda.EtiquetasAlternativas);
                    etiquetas.AddRange(celda.Valor.AlternativasTextuales);
                }
            }
        }

        foreach (ReglaMatrizEsperada regla in caso.MatricesEsperadas) {
            nombres.Add(regla.Nombre);
            etiquetas.AddRange(regla.EtiquetasInicio);
            separadores.AddRange(regla.SeparadoresColumnas);
            etiquetas.AddRange(
                regla.ValoresTextualesEsperados.SelectMany(fila => fila));
        }

        foreach (ReglaBloquesRegistroEsperados regla
            in caso.BloquesRegistroEsperados) {
            nombres.Add(regla.Nombre);
            nombres.Add(regla.NombreCampoClave);
            etiquetas.AddRange(regla.EtiquetasClave);

            foreach (RegistroEsperado registro in regla.RegistrosEsperados) {
                nombres.Add(registro.Nombre);
                nombres.Add(registro.Clave.Nombre);
                etiquetas.AddRange(registro.Clave.AlternativasTextuales);

                foreach (CampoRegistroEsperado campo in registro.Campos) {
                    nombres.Add(campo.Nombre);
                    nombres.Add(campo.Valor.Nombre);
                    etiquetas.AddRange(campo.EtiquetasAlternativas);
                    etiquetas.AddRange(campo.Valor.AlternativasTextuales);
                }
            }
        }

        Assert.All(
            nombres,
            nombre => Assert.False(string.IsNullOrWhiteSpace(nombre)));
        Assert.All(
            etiquetas,
            etiqueta => Assert.False(string.IsNullOrWhiteSpace(etiqueta)));
        Assert.All(
            separadores,
            separador => Assert.False(string.IsNullOrEmpty(separador)));
    }

    private static void AgregarEventos(
        IEnumerable<ElementoTextualSecuenciaEsperado> eventos,
        List<string> nombres,
        List<string> etiquetas) {
        foreach (ElementoTextualSecuenciaEsperado evento in eventos) {
            nombres.Add(evento.Valor);
            etiquetas.AddRange(evento.Alternativas);
            etiquetas.AddRange(evento.EtiquetasNumericasAsociadas);
            etiquetas.AddRange(evento.EtiquetasNumericasPosteriores);
        }
    }

    private static void AssertToleranciasValidas(CasoPrueba caso) {
        Assert.All(
            caso.ValoresNumericosEsperados,
            valor => {
                Assert.True(double.IsFinite(valor.Valor));
                AssertTolerancia(valor.Tolerancia);
                Assert.All(
                    valor.ValoresEquivalentes,
                    equivalente => Assert.True(double.IsFinite(equivalente)));
            });

        foreach (SecuenciaEsperada secuencia in caso.SecuenciasEsperadas) {
            AssertTolerancia(secuencia.ToleranciaNumerica);
            Assert.All(
                secuencia.ValoresNumericosEsperados,
                valor => Assert.True(double.IsFinite(valor)));
            Assert.All(
                secuencia.AlternativasTextualesEsperadas
                    .Concat(secuencia.EventosTextualesReconocibles)
                    .Where(evento => evento.ValorNumericoAsociado.HasValue),
                evento => Assert.True(
                    double.IsFinite(evento.ValorNumericoAsociado!.Value)));
        }

        foreach (SecuenciaCompuestaEsperada secuencia
            in caso.SecuenciasCompuestasEsperadas) {
            Assert.All(
                secuencia.PasosEsperados.SelectMany(paso => paso.Componentes),
                componente => {
                    Assert.True(double.IsFinite(componente.Valor));
                    AssertTolerancia(componente.Tolerancia);
                });
        }

        foreach (ReglaColeccionEsperada regla in caso.ColeccionesEsperadas) {
            AssertTolerancia(regla.ToleranciaNumerica);
            Assert.All(
                regla.ElementosEsperados,
                AssertValorEstructuradoValido);
        }

        foreach (ReglaTablaEsperada regla in caso.TablasEsperadas) {
            Assert.All(
                regla.FilasEsperadas
                    .SelectMany(fila => fila.Celdas)
                    .Select(celda => celda.Valor),
                AssertValorEstructuradoValido);
        }

        foreach (ReglaMatrizEsperada regla in caso.MatricesEsperadas) {
            AssertTolerancia(regla.ToleranciaNumerica);
            Assert.All(
                regla.ValoresNumericosEsperados.SelectMany(fila => fila),
                valor => Assert.True(double.IsFinite(valor)));
        }

        foreach (ReglaBloquesRegistroEsperados regla
            in caso.BloquesRegistroEsperados) {
            Assert.All(regla.RegistrosEsperados, registro => {
                AssertValorEstructuradoValido(registro.Clave);
                Assert.All(
                    registro.Campos.Select(campo => campo.Valor),
                    AssertValorEstructuradoValido);
            });
        }
    }

    private static void AssertValorEstructuradoValido(
        ValorEstructuradoEsperado valor) {
        AssertTolerancia(valor.ToleranciaNumerica);
        if (valor.Tipo == TipoValorEstructurado.Numerico) {
            Assert.True(double.IsFinite(valor.ValorNumerico));
        }
    }

    private static void AssertTolerancia(double tolerancia) {
        Assert.True(double.IsFinite(tolerancia));
        Assert.True(tolerancia >= 0D);
    }

    private static void AssertCantidadesValidas(CasoPrueba caso) {
        Assert.All(
            caso.SecuenciasEsperadas,
            secuencia => {
                if (secuencia.CantidadExacta.HasValue) {
                    Assert.True(secuencia.CantidadExacta.Value > 0);
                }
            });
        Assert.All(
            caso.SecuenciasCompuestasEsperadas,
            secuencia => {
                if (secuencia.CantidadExacta.HasValue) {
                    Assert.True(secuencia.CantidadExacta.Value > 0);
                }

                Assert.NotEmpty(secuencia.PasosEsperados);
            });
        Assert.All(
            caso.ColeccionesEsperadas,
            coleccion => {
                if (coleccion.CantidadExacta.HasValue) {
                    Assert.True(coleccion.CantidadExacta.Value >= 0);
                    Assert.Equal(
                        coleccion.ElementosEsperados.Count,
                        coleccion.CantidadExacta.Value);
                }
            });
        Assert.All(
            caso.TablasEsperadas,
            tabla => {
                if (tabla.CantidadFilasExacta.HasValue) {
                    Assert.True(tabla.CantidadFilasExacta.Value > 0);
                    Assert.Equal(
                        tabla.FilasEsperadas.Count,
                        tabla.CantidadFilasExacta.Value);
                }

                if (tabla.CantidadColumnasExacta.HasValue) {
                    Assert.True(tabla.CantidadColumnasExacta.Value > 0);
                }
            });
        Assert.All(
            caso.MatricesEsperadas,
            matriz => {
                Assert.True(matriz.FilasEsperadas > 0);
                Assert.True(matriz.ColumnasEsperadas > 0);
            });
    }

    private static void AssertRutasArchivosSeguras(CasoPrueba caso) {
        ArchivoEntradaPrueba[] entradas = caso.ArchivosEntrada.ToArray();
        ArchivoEsperadoPrueba[] esperados = caso.ArchivosEsperados.ToArray();

        AssertRutasUnicasYSeguras(entradas.Select(archivo => archivo.RutaRelativa));
        AssertRutasUnicasYSeguras(esperados.Select(archivo => archivo.RutaRelativa));
    }

    private static void AssertRutasUnicasYSeguras(
        IEnumerable<string> rutasConfiguradas) {
        string[] rutas = rutasConfiguradas.ToArray();

        Assert.Equal(
            rutas.Length,
            rutas.Distinct(StringComparer.OrdinalIgnoreCase).Count());

        Assert.All(rutas, ruta => {
            Assert.False(string.IsNullOrWhiteSpace(ruta));
            Assert.False(Path.IsPathRooted(ruta));
            Assert.DoesNotContain(':', ruta);

            string[] segmentos = ruta
                .Replace('\\', '/')
                .Split('/', StringSplitOptions.None);
            Assert.All(segmentos, segmento => {
                Assert.False(string.IsNullOrWhiteSpace(segmento));
                Assert.NotEqual(".", segmento);
                Assert.NotEqual("..", segmento);
            });
        });
    }
}
