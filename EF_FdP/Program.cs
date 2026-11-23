using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace MesaPartesSacoOliveros;

// Un documento que alguien entrega en mesa de partes.
internal class Expediente
{
    public string Codigo { get; set; } = "";
    public string Remitente { get; set; } = "";
    public string Dni { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string TipoDocumento { get; set; } = "";
    public string Asunto { get; set; } = "";
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public string Estado { get; set; } = "Registrado";
}

// Una cita agendada con el colegio.
internal class Cita
{
    public string Codigo { get; set; } = "";
    public string Paciente { get; set; } = "";
    public string Dni { get; set; } = "";
    public string Telefono { get; set; } = "";
    public DateTime Fecha { get; set; }
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFin { get; set; }
    public string Motivo { get; set; } = "";
    public string Estado { get; set; } = "Activa";
    public string ArchivoComprobante { get; set; } = "";
}

internal class Program
{
    // Dónde se guardan los datos y los comprobantes en la computadora.
    private static readonly string CarpetaDatos =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MesaPartesSacoOliveros");
    private static readonly string ArchivoExpedientes = Path.Combine(CarpetaDatos, "expedientes.json");
    private static readonly string ArchivoCitas = Path.Combine(CarpetaDatos, "citas.json");
    private static readonly string CarpetaComprobantes =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MesaPartesSacoOliveros", "Comprobantes");

    // Las listas donde el programa guarda todo mientras está abierto.
    private static List<Expediente> expedientes = new List<Expediente>();
    private static List<Cita> citas = new List<Cita>();

    // Aquí empieza el programa: carga los datos y muestra el menú una y otra vez hasta que elijan 9.
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        CargarDatos();

        while (true)
        {
            Titulo("COLEGIO SACO OLIVEROS - MESA DE PARTES Y AGENDA DE CITAS");
            Console.WriteLine("1. Registrar documento");
            Console.WriteLine("2. Buscar documento");
            Console.WriteLine("3. Mostrar documentos");
            Console.WriteLine("4. Ordenar documentos");
            Console.WriteLine("5. Agendar cita");
            Console.WriteLine("6. Ver agenda");
            Console.WriteLine("7. Buscar cita");
            Console.WriteLine("8. Cancelar cita");
            Console.WriteLine("9. Salir");

            switch (Leer("\nSeleccione una opción: "))
            {
                case "1": RegistrarDocumento(); break;
                case "2": BuscarDocumento(); break;
                case "3": MostrarDocumentos(); break;
                case "4": OrdenarDocumentos(); break;
                case "5": AgendarCita(); break;
                case "6": VerAgenda(); break;
                case "7": BuscarCita(); break;
                case "8": CancelarCita(); break;
                case "9": Console.WriteLine("Gracias por utilizar el sistema."); return;
                default: Console.WriteLine("Opción no válida."); break;
            }

            Pausa();
        }
    }

    // ---------- Archivos JSON ----------

    // Lee los archivos guardados para no perder lo de la vez anterior.
    private static void CargarDatos()
    {
        try
        {
            // Si el archivo existe, convierte el texto JSON en la lista. "?? new List" = si sale vacío, usa lista nueva.
            if (File.Exists(ArchivoExpedientes))
                expedientes = JsonSerializer.Deserialize<List<Expediente>>(File.ReadAllText(ArchivoExpedientes)) ?? new List<Expediente>();
            if (File.Exists(ArchivoCitas))
                citas = JsonSerializer.Deserialize<List<Cita>>(File.ReadAllText(ArchivoCitas)) ?? new List<Cita>();
        }
        catch
        {
            Console.WriteLine("Advertencia: no se pudieron leer los datos guardados. Se iniciará con listas vacías.");
            Pausa();
        }
    }

    // Guarda las dos listas en archivos. Se llama después de cada cambio.
    private static void GuardarDatos()
    {
        try
        {
            var opciones = new JsonSerializerOptions { WriteIndented = true };
            Directory.CreateDirectory(CarpetaDatos);
            File.WriteAllText(ArchivoExpedientes, JsonSerializer.Serialize(expedientes, opciones));
            File.WriteAllText(ArchivoCitas, JsonSerializer.Serialize(citas, opciones));
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al guardar los datos: " + ex.Message);
        }
    }

    // ---------- Documentos ----------

    // Opción 1: pide los datos, crea el documento, lo agrega a la lista y guarda.
    private static void RegistrarDocumento()
    {
        Titulo("REGISTRO DE DOCUMENTO");
        var e = new Expediente
        {
            Codigo = "DOC-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
            Remitente = PedirTexto("Nombre completo del remitente: "),
            Dni = PedirDigitos("DNI", 8),
            Telefono = PedirDigitos("Teléfono", 9),
            TipoDocumento = PedirTexto("Tipo de documento (solicitud, oficio, carta, etc.): "),
            Asunto = PedirTexto("Asunto o descripción: ")
        };

        expedientes.Add(e);
        GuardarDatos();
        Console.WriteLine("\nDocumento registrado correctamente.");
        Console.WriteLine("Código asignado: " + e.Codigo);
    }

    // Opción 2: revisa documento por documento y muestra los que coinciden.
    private static void BuscarDocumento()
    {
        Titulo("BUSCAR DOCUMENTO");
        string dato = Leer("Ingrese código, DNI o nombre del remitente: ").ToLower();
        bool encontrado = false;

        foreach (Expediente e in expedientes)
        {
            if (e.Codigo.ToLower().Contains(dato) || e.Dni.Contains(dato) || e.Remitente.ToLower().Contains(dato))
            {
                MostrarExpediente(e);
                encontrado = true;
            }
        }

        if (!encontrado) Console.WriteLine("No se encontraron documentos.");
    }

    // Opción 3: muestra todos los documentos.
    private static void MostrarDocumentos()
    {
        Titulo("LISTA DE DOCUMENTOS");
        if (expedientes.Count == 0) Console.WriteLine("Todavía no hay documentos registrados.");
        foreach (var e in expedientes) MostrarExpediente(e);
    }

    // Opción 4: ordena la lista por fecha o por nombre.
    private static void OrdenarDocumentos()
    {
        Titulo("ORDENAR DOCUMENTOS");
        Console.WriteLine("1. Por fecha (más recientes primero)");
        Console.WriteLine("2. Por nombre del remitente");

        // "e => e.FechaRegistro" se lee: "de cada expediente e, usa su fecha para ordenar".
        switch (Leer("Seleccione: "))
        {
            case "1": expedientes = expedientes.OrderByDescending(e => e.FechaRegistro).ToList(); break;
            case "2": expedientes = expedientes.OrderBy(e => e.Remitente).ToList(); break;
            default: Console.WriteLine("Opción no válida."); return;
        }

        GuardarDatos();
        Console.WriteLine("\nDocumentos ordenados correctamente.");
        foreach (var e in expedientes)
            Console.WriteLine($"{e.Codigo} | {e.Remitente} | {e.FechaRegistro:dd/MM/yyyy HH:mm}");
    }

    // Imprime un documento en pantalla.
    private static void MostrarExpediente(Expediente e)
    {
        Console.WriteLine("----------------------------------------");
        Console.WriteLine($"Código: {e.Codigo}");
        Console.WriteLine($"Remitente: {e.Remitente}");
        Console.WriteLine($"DNI: {e.Dni} | Teléfono: {e.Telefono}");
        Console.WriteLine($"Tipo: {e.TipoDocumento}");
        Console.WriteLine($"Asunto: {e.Asunto}");
        Console.WriteLine($"Registro: {e.FechaRegistro:dd/MM/yyyy HH:mm}");
        Console.WriteLine($"Estado: {e.Estado}");
    }

    // ---------- Citas ----------

    // Opción 5: pide los datos, revisa el horario, crea el comprobante .txt y guarda.
    private static void AgendarCita()
    {
        Titulo("AGENDAR CITA");
        var c = new Cita
        {
            Codigo = "CIT-" + DateTime.Now.ToString("yyyyMMddHHmmssfff"),
            Paciente = PedirTexto("Nombre completo del paciente: "),
            Dni = PedirDigitos("DNI", 8),
            Telefono = PedirDigitos("Teléfono", 9),
            Fecha = PedirFecha(),
            HoraInicio = PedirHora("Hora de inicio (HH:mm): "),
            HoraFin = PedirHora("Hora de término (HH:mm): ")
        };

        string error = ValidarHorario(c);
        if (error != "")
        {
            Console.WriteLine(error);
            return;
        }

        c.Motivo = PedirTexto("Motivo de la cita: ");
        c.ArchivoComprobante = Path.Combine(CarpetaComprobantes, $"Cita_{c.Codigo}.txt");
        Directory.CreateDirectory(CarpetaComprobantes);
        File.WriteAllText(c.ArchivoComprobante, CrearComprobante(c), Encoding.UTF8);

        citas.Add(c);
        GuardarDatos();
        Console.WriteLine("\nCita registrada correctamente.");
        Console.WriteLine("Código: " + c.Codigo);
        Console.WriteLine("Comprobante: " + c.ArchivoComprobante);

        try
        {
            // Abre el comprobante en el Bloc de notas.
            Process.Start(new ProcessStartInfo("notepad.exe", $"\"{c.ArchivoComprobante}\"") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine("No se pudo abrir el Bloc de notas automáticamente: " + ex.Message);
        }
    }

    // Reglas del horario. Devuelve el mensaje de error, o "" (vacío) si todo está bien.
    private static string ValidarHorario(Cita c)
    {
        if (c.HoraFin <= c.HoraInicio)
            return "La hora de término debe ser posterior a la de inicio.";
        if (c.HoraInicio < new TimeSpan(8, 0, 0) || c.HoraFin > new TimeSpan(17, 0, 0))
            return "El horario de atención es de 08:00 a 17:00.";
        if ((c.HoraFin - c.HoraInicio).TotalMinutes < 15)
            return "La cita debe durar como mínimo 15 minutos.";
        if (c.Fecha == DateTime.Today && c.HoraInicio <= DateTime.Now.TimeOfDay)
            return "La hora de inicio debe ser posterior a la hora actual.";

        // Se cruza si empieza antes de que la otra termine y termina después de que la otra empiece.
        foreach (Cita otra in citas)
        {
            if (otra.Estado == "Activa" && otra.Fecha.Date == c.Fecha && c.HoraInicio < otra.HoraFin && c.HoraFin > otra.HoraInicio)
                return "Ese horario se cruza con otra cita activa. Elija otro horario.";
        }

        return "";
    }

    // Arma el texto del comprobante. "hh\\:mm" muestra la hora como 09:30.
    private static string CrearComprobante(Cita c)
    {
        return
            "COLEGIO SACO OLIVEROS\n" +
            "COMPROBANTE DE CITA\n" +
            "========================================\n" +
            $"Código: {c.Codigo}\n" +
            $"Paciente: {c.Paciente}\n" +
            $"DNI: {c.Dni}\n" +
            $"Teléfono: {c.Telefono}\n" +
            $"Fecha: {c.Fecha:dd/MM/yyyy}\n" +
            $"Hora de inicio: {c.HoraInicio:hh\\:mm}\n" +
            $"Hora de término: {c.HoraFin:hh\\:mm}\n" +
            $"Motivo: {c.Motivo}\n" +
            $"Estado: {c.Estado}\n" +
            $"Fecha de emisión: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n" +
            "========================================\n" +
            "Conserve este comprobante para su atención.\n";
    }

    // Opción 6: muestra todas las citas ordenadas por fecha y hora.
    private static void VerAgenda()
    {
        Titulo("AGENDA DE CITAS");
        if (citas.Count == 0) Console.WriteLine("No hay citas registradas.");
        foreach (var c in citas.OrderBy(c => c.Fecha).ThenBy(c => c.HoraInicio)) MostrarCita(c);
    }

    // Opción 7: revisa cita por cita y muestra las que coinciden.
    private static void BuscarCita()
    {
        Titulo("BUSCAR CITA");
        string dato = Leer("Ingrese código, DNI o nombre del paciente: ").ToLower();
        bool encontrada = false;

        foreach (Cita c in citas)
        {
            if (c.Codigo.ToLower().Contains(dato) || c.Dni.Contains(dato) || c.Paciente.ToLower().Contains(dato))
            {
                MostrarCita(c);
                encontrada = true;
            }
        }

        if (!encontrada) Console.WriteLine("No se encontraron citas.");
    }

    // Opción 8: busca la cita por código y le cambia el estado a "Cancelada".
    private static void CancelarCita()
    {
        Titulo("CANCELAR CITA");
        string codigo = Leer("Ingrese el código de la cita: ").ToUpper();

        foreach (Cita c in citas)
        {
            if (c.Codigo.ToUpper() == codigo)
            {
                if (c.Estado != "Activa")
                {
                    Console.WriteLine("La cita ya se encuentra " + c.Estado.ToLower() + ".");
                }
                else
                {
                    c.Estado = "Cancelada";
                    GuardarDatos();
                    Console.WriteLine("Cita cancelada correctamente. El comprobante original se conserva.");
                }
                return;
            }
        }

        Console.WriteLine("No existe una cita con ese código.");
    }

    // Imprime una cita en pantalla.
    private static void MostrarCita(Cita c)
    {
        Console.WriteLine("----------------------------------------");
        Console.WriteLine($"Código: {c.Codigo}");
        Console.WriteLine($"Paciente: {c.Paciente} | DNI: {c.Dni}");
        Console.WriteLine($"Teléfono: {c.Telefono}");
        Console.WriteLine($"Fecha: {c.Fecha:dd/MM/yyyy}");
        Console.WriteLine($"Horario: {c.HoraInicio:hh\\:mm} - {c.HoraFin:hh\\:mm}");
        Console.WriteLine($"Motivo: {c.Motivo}");
        Console.WriteLine($"Estado: {c.Estado}");
        if (c.ArchivoComprobante != "") Console.WriteLine($"Comprobante: {c.ArchivoComprobante}");
    }

    // ---------- Entrada por teclado ----------

    // Muestra el mensaje y devuelve lo que escribió el usuario, sin espacios a los lados.
    private static string Leer(string mensaje)
    {
        Console.Write(mensaje);
        return (Console.ReadLine() ?? "").Trim();
    }

    // Pregunta hasta que escriban algo (no deja vacío).
    private static string PedirTexto(string mensaje)
    {
        while (true)
        {
            string valor = Leer(mensaje);
            if (valor != "") return valor;
            Console.WriteLine("Este campo no puede quedar vacío.");
        }
    }

    // Sirve para DNI (8 dígitos) y teléfono (9 dígitos).
    private static string PedirDigitos(string campo, int cantidad)
    {
        while (true)
        {
            string valor = Leer($"{campo} ({cantidad} dígitos): ");
            bool soloNumeros = true;
            foreach (char letra in valor)
            {
                if (!char.IsDigit(letra)) soloNumeros = false;
            }

            if (valor.Length == cantidad && soloNumeros) return valor;
            Console.WriteLine($"Debe ingresar exactamente {cantidad} números.");
        }
    }

    // Pregunta hasta que escriban una fecha válida que no sea del pasado.
    private static DateTime PedirFecha()
    {
        while (true)
        {
            string entrada = Leer("Fecha de la cita (dd/MM/yyyy): ");
            // TryParseExact intenta convertir el texto en fecha: devuelve true si pudo y la guarda en "fecha".
            if (DateTime.TryParseExact(entrada, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha)
                && fecha >= DateTime.Today)
                return fecha;
            Console.WriteLine("Fecha inválida. Use dd/MM/yyyy y no puede ser anterior al día de hoy.");
        }
    }

    // Pregunta hasta que escriban una hora válida como 09:30.
    private static TimeSpan PedirHora(string mensaje)
    {
        while (true)
        {
            string entrada = Leer(mensaje);
            if (DateTime.TryParseExact(entrada, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime hora))
                return hora.TimeOfDay;
            Console.WriteLine("Formato incorrecto. Use HH:mm, por ejemplo 09:30.");
        }
    }

    // ---------- Pantalla ----------

    // Limpia la pantalla y pone un título arriba.
    private static void Titulo(string texto)
    {
        Console.Clear();
        Console.WriteLine("==============================================================");
        Console.WriteLine(" " + texto);
        Console.WriteLine("==============================================================\n");
    }

    // Espera a que presionen ENTER.
    private static void Pausa()
    {
        Console.WriteLine("\nPresione ENTER para continuar...");
        Console.ReadLine();
    }
}