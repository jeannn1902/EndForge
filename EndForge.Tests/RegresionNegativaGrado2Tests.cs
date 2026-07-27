using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class RegresionNegativaGrado2Tests {
    private static readonly ComparadorSalidaService Comparador = new();
    private static readonly EvaluacionPracticaService Evaluador = new();

    [Theory]
    [InlineData("Suma total: 11")]
    [InlineData("")]
    [InlineData("Suma total: 10\nTotal: 11")]
    public void Valores_RechazanResultadoIncorrectoAusenteOContradictorio(
        string salida) {
        CasoPrueba caso = ObtenerCaso(
            CatalogoEvaluacionesService.ArreglosSumaElementosId);

        ResultadoComparacionSalida resultado = Comparador.Comparar(
            caso,
            salida);

        Assert.False(resultado.EsCorrecta);
    }

    [Theory]
    [InlineData("Arreglo: -1 3 3 0")]
    [InlineData("Arreglo: 3 -1 3")]
    [InlineData("Arreglo: 3 -1 3 0 99")]
    [InlineData("Arreglo: 3 -1 3 3 0")]
    public void Colecciones_RechazanOrdenFaltantesExtrasYDuplicados(
        string salida) {
        CasoPrueba caso = ObtenerCaso(
            CatalogoEvaluacionesService.ArreglosCapturarMostrarId);

        ResultadoComparacionSalida resultado = Comparador.Comparar(
            caso,
            salida);

        Assert.False(resultado.EsCorrecta);
    }

    [Theory]
    [InlineData("Texto: HOLA MUNDO")]
    [InlineData("Texto: Hola  mundo")]
    [InlineData("Texto: Hola")]
    public void Cadenas_RechazanMayusculasEspaciosYTextoTruncado(
        string salida) {
        CasoPrueba caso = ObtenerCaso(
            CatalogoEvaluacionesService.CadenasCapturarMostrarId);

        ResultadoComparacionSalida resultado = Comparador.Comparar(
            caso,
            salida);

        Assert.False(resultado.EsCorrecta);
    }

    [Fact]
    public void Cadenas_RechazanUnaInversionQueNoEsPorCaracteres() {
        CasoPrueba caso = ObtenerCaso(
            CatalogoEvaluacionesService.CadenasInvertirId);

        ResultadoComparacionSalida resultado = Comparador.Comparar(
            caso,
            "Invertida: Hola");

        Assert.False(resultado.EsCorrecta);
    }

    [Theory]
    [InlineData("1 2 3 4 5 6")]
    [InlineData("1 2\n3 4\n5 6")]
    [InlineData("1 4\n2 5\n3 6")]
    [InlineData("1 2 3\n4 5")]
    [InlineData("1 2 3\n4 5 6\n7 8 9")]
    public void Matrices_RechazanFormaOPosicionesIncorrectas(
        string salida) {
        CasoPrueba caso = ObtenerCaso(
            CatalogoEvaluacionesService.MatricesCapturarMostrarId);

        ResultadoComparacionSalida resultado = Comparador.Comparar(
            caso,
            salida);

        Assert.False(resultado.EsCorrecta);
    }

    [Fact]
    public void Matrices_RechazanMultiplicacionElementoAElemento() {
        CasoPrueba caso = ObtenerCaso(
            CatalogoEvaluacionesService.MatricesMultiplicarId);

        ResultadoComparacionSalida resultado = Comparador.Comparar(
            caso,
            "Producto:\n5 12\n21 32");

        Assert.False(resultado.EsCorrecta);
    }

    [Theory]
    [InlineData(
        "ID: 10\nNombre: Ana López\nPromedio: 8.5")]
    [InlineData(
        "ID: 10\nNombre: Ana López\nPromedio: 8.5\n" +
        "ID: 20\nNombre: Luis Pérez\nPromedio: 7\n" +
        "ID: 30\nNombre: Extra\nPromedio: 10")]
    [InlineData(
        "ID: 10\nNombre: Ana López\nPromedio: 8.5\n" +
        "ID: 10\nNombre: Luis Pérez\nPromedio: 7")]
    [InlineData(
        "ID: 20\nNombre: Luis Pérez\nPromedio: 7\n" +
        "ID: 10\nNombre: Ana López\nPromedio: 8.5")]
    [InlineData(
        "ID: 10\nNombre: Luis Pérez\nPromedio: 8.5\n" +
        "ID: 20\nNombre: Ana López\nPromedio: 7")]
    public void Registros_RechazanFaltantesExtrasDuplicadosOrdenYMezcla(
        string salida) {
        CasoPrueba caso = ObtenerCaso(
            CatalogoEvaluacionesService.EstructurasArregloEstudiantesId);

        ResultadoComparacionSalida resultado = Comparador.Comparar(
            caso,
            salida);

        Assert.False(resultado.EsCorrecta);
    }

    [Fact]
    public void Registros_RechazanUnEmpateInestable() {
        CasoPrueba caso = ObtenerCaso(
            CatalogoEvaluacionesService.EstructurasMejorPromedioId,
            indice: 2);

        ResultadoComparacionSalida resultado = Comparador.Comparar(
            caso,
            "ID: 2\nNombre: Luis\nPromedio: 8");

        Assert.False(resultado.EsCorrecta);
    }

    [Theory]
    [InlineData("10|Ana López|8.5")]
    [InlineData(
        "10|Ana López|8.5\n20|Luis Pérez|7\n30|Extra|10")]
    [InlineData("20|Luis Pérez|7\n10|Ana López|8.5")]
    [InlineData("11|Ana López|8.5\n20|Luis Pérez|7")]
    public void TablasEnArchivo_RechazanFilasInvalidas(string contenido) {
        CasoPrueba caso = ObtenerCaso(
            CatalogoEvaluacionesService.ArchivosGuardarEstudiantesId);
        ArchivoEsperadoPrueba esperado = Assert.Single(
            caso.ArchivosEsperados);

        ResultadoComparacionSalida resultado =
            Evaluador.CompararResultadoCaso(
                caso,
                string.Empty,
                new[] {
                    ArchivoDisponible(esperado.RutaRelativa, contenido)
                });

        Assert.False(resultado.EsCorrecta);
    }

    [Fact]
    public void Archivos_RechazanContenidoTruncado() {
        CasoPrueba caso = ObtenerCaso(
            CatalogoEvaluacionesService.ArchivosEscribirTextoId);
        ArchivoEsperadoPrueba esperado = Assert.Single(
            caso.ArchivosEsperados);

        ResultadoComparacionSalida resultado =
            Evaluador.CompararResultadoCaso(
                caso,
                caso.SalidaEsperada,
                new[] {
                    ArchivoDisponible(
                        esperado.RutaRelativa,
                        esperado.ContenidoEsperado[..^1])
                });

        Assert.False(resultado.EsCorrecta);
    }

    [Fact]
    public void Archivos_RechazanArchivoAusenteONombreIncorrecto() {
        CasoPrueba caso = ObtenerCaso(
            CatalogoEvaluacionesService.ArchivosEscribirTextoId);

        ResultadoComparacionSalida ausente =
            Evaluador.CompararResultadoCaso(
                caso,
                caso.SalidaEsperada,
                Array.Empty<ResultadoArchivoPrueba>());
        ResultadoComparacionSalida nombreIncorrecto =
            Evaluador.CompararResultadoCaso(
                caso,
                caso.SalidaEsperada,
                new[] {
                    ArchivoDisponible(
                        "otro-nombre.txt",
                        "Hola EndForge")
                });

        Assert.False(ausente.EsCorrecta);
        Assert.False(nombreIncorrecto.EsCorrecta);
    }

    private static CasoPrueba ObtenerCaso(
        string practicaId,
        int indice = 0) {
        return CanonicalEvaluationFactory
            .ObtenerDefinicion(practicaId)
            .CasosPrueba[indice];
    }

    private static ResultadoArchivoPrueba ArchivoDisponible(
        string ruta,
        string contenido) {
        return new ResultadoArchivoPrueba {
            RutaRelativa = ruta,
            Estado = EstadoArchivoPrueba.Disponible,
            ContenidoObtenido = contenido
        };
    }
}
