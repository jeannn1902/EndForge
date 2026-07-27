using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

internal static class CatalogoTestHelper {
    internal static readonly string[] IdsPracticasGradoUno = {
        "variables-datos-personales",
        "variables-ticket-compra",
        "variables-conversor-temperatura",
        "variables-promedio-calificaciones",
        "variables-mini-recibo",
        "condicionales-mayor-de-edad",
        "condicionales-clasificar-numero",
        "condicionales-calificacion-aprobatoria",
        "condicionales-descuento-compra",
        "condicionales-menu-operaciones",
        "ciclos-contar-uno-a-diez",
        "ciclos-tabla-multiplicar",
        "ciclos-suma-acumulada",
        "ciclos-adivina-numero",
        "ciclos-menu-repetitivo",
        "funciones-saludo-personalizado",
        "funciones-sumar-dos-numeros",
        "funciones-numero-par",
        "funciones-calcular-promedio",
        "funciones-calculadora-modular"
    };

    internal static readonly string[] IdsPracticasGradoDos = {
        "grado2-arreglos-capturar-mostrar",
        "grado2-arreglos-suma-elementos",
        "grado2-arreglos-promedio",
        "grado2-arreglos-mayor-menor",
        "grado2-arreglos-contar-pares-impares",
        "grado2-arreglos-buscar-valor",
        "grado2-arreglos-invertir",
        "grado2-arreglos-intercalar",
        "grado2-arreglos-sin-duplicados",
        "grado2-arreglos-ordenar-segundo-mayor",
        "grado2-cadenas-capturar-mostrar",
        "grado2-cadenas-longitud",
        "grado2-cadenas-mayusculas-minusculas",
        "grado2-cadenas-invertir",
        "grado2-cadenas-palindromo",
        "grado2-cadenas-contar-caracteres",
        "grado2-cadenas-reemplazar-caracter",
        "grado2-cadenas-contar-palabras",
        "grado2-matrices-capturar-mostrar",
        "grado2-matrices-suma-elementos",
        "grado2-matrices-sumas-filas-columnas",
        "grado2-matrices-diagonales",
        "grado2-matrices-transpuesta",
        "grado2-matrices-sumar-dos",
        "grado2-matrices-multiplicar",
        "grado2-matrices-mayor-menor-posicion",
        "grado2-estructuras-datos-estudiante",
        "grado2-estructuras-promedio-estudiante",
        "grado2-estructuras-arreglo-estudiantes",
        "grado2-estructuras-buscar-estudiante",
        "grado2-estructuras-mejor-promedio",
        "grado2-estructuras-ordenar-estudiantes",
        "grado2-estructuras-inventario-productos",
        "grado2-estructuras-registro-empleados",
        "grado2-archivos-escribir-texto",
        "grado2-archivos-leer-texto",
        "grado2-archivos-contar-lineas-palabras",
        "grado2-archivos-guardar-estudiantes",
        "grado2-archivos-buscar-registro",
        "grado2-archivos-resumen-numerico"
    };

    internal static CursoService CrearCursoGradoUno() {
        return new CursoService();
    }

    internal static CursoService CrearCursoGradoDos() {
        GradosService gradosService = new(CrearCursoGradoUno());
        CursoService? curso = gradosService.ObtenerCurso(
            GradosService.GradoJuniorId);

        return Assert.IsType<CursoService>(curso);
    }

    internal static PracticaCurso[] CargarPracticas(CursoService curso) {
        return curso.CargarTemas()
            .SelectMany(tema => tema.Practicas)
            .ToArray();
    }

    internal static DefinicionEvaluacionPractica[] CargarDefiniciones(
        IEnumerable<string> idsPracticas) {
        HashSet<string> ids = idsPracticas.ToHashSet(
            StringComparer.OrdinalIgnoreCase);

        return new CatalogoEvaluacionesService()
            .CargarDefiniciones()
            .Where(definicion => ids.Contains(definicion.PracticaId))
            .ToArray();
    }
}
