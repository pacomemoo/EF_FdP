using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace MesaPartesSacoOliveros
{
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
        private static readonly string CarpetaDatos =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MesaPartesSacoOliveros");

        private static readonly string ArchivoExpedientes = Path.Combine(CarpetaDatos, "expedientes.json");
        private static readonly string ArchivoCitas = Path.Combine(CarpetaDatos, "citas.json");
        private static readonly string CarpetaComprobantes =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MesaPartesSacoOliveros", "Comprobantes");

        private static readonly JsonSerializerOptions JsonOpciones = new JsonSerializerOptions { WriteIndented = true };

        private static List<Expediente> expedientes = new List<Expediente>();
        private static List<Cita> citas = new List<Cita>();

        static void Main()
        {
            Console.Title = "Mesa de Partes Digital y Agenda de Citas - Colegio Saco Oliveros";
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            CargarDatos();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("==============================================================");
                Console.WriteLine("       COLEGIO SACO OLIVEROS - MESA DE PARTES DIGITAL");
                Console.WriteLine("                  Y AGENDA DE CITAS");
                Console.WriteLine("==============================================================");
                Console.WriteLine("1. Registrar documento");
                Console.WriteLine("2. Buscar documento");
                Console.WriteLine("3. Mostrar documentos");
                Console.WriteLine("4. Ordenar documentos");
                Console.WriteLine("5. Agendar cita");
                Console.WriteLine("6. Ver agenda");
                Console.WriteLine("7. Buscar cita");
                Console.WriteLine("8. Cancelar cita");
                Console.WriteLine("9. Salir");
                Console.WriteLine("--------------------------------------------------------------");
                Console.Write("Seleccione una opción: ");
                string opcion = (Console.ReadLine() ?? "").Trim();

                switch (opcion)
                {
                    case "1": RegistrarDocumento(); break;
                    case "2": BuscarDocumento(); break;
                    case "3": MostrarDocumentos(); break;
                    case "4": OrdenarDocumentos(); break;
                    case "5": AgendarCita(); break;
                    case "6": VerAgenda(); break;
                    case "7": BuscarCita(); break;
                    case "8": CancelarCita(); break;
                    case "9":
                        GuardarDatos();
                        Console.WriteLine("Gracias por utilizar el sistema.");
                        return;
                    default:
                        Console.WriteLine("Opción no válida.");
                        Pausa();
                        break;
                }
            }
        }

        private static void CargarDatos()
        {
            Directory.CreateDirectory(CarpetaDatos);
            Directory.CreateDirectory(CarpetaComprobantes);
            expedientes = LeerLista<Expediente>(ArchivoExpedientes);
            citas = LeerLista<Cita>(ArchivoCitas);
        }

        private static List<T> LeerLista<T>(string ruta)
        {
            try
            {
                if (!File.Exists(ruta)) return new List<T>();
                return JsonSerializer.Deserialize<List<T>>(File.ReadAllText(ruta)) ?? new List<T>();
            }
            catch
            {
                Console.WriteLine("Advertencia: no se pudo leer " + Path.GetFileName(ruta) + ". Se iniciará una lista vacía.");
                return new List<T>();
            }
        }

        private static void GuardarDatos()
        {
            Directory.CreateDirectory(CarpetaDatos);
            File.WriteAllText(ArchivoExpedientes, JsonSerializer.Serialize(expedientes, JsonOpciones));
            File.WriteAllText(ArchivoCitas, JsonSerializer.Serialize(citas, JsonOpciones));
        }

        private static void RegistrarDocumento()
        {
            Console.Clear();
            Titulo("REGISTRO DE DOCUMENTO");
            Expediente e = new Expediente();
            e.Codigo = "DOC-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            e.Remitente = PedirTexto("Nombre completo del remitente: ");
            e.Dni = PedirDni();
            e.Telefono = PedirTelefono();
            e.TipoDocumento = PedirTexto("Tipo de documento (solicitud, oficio, carta, etc.): ");
            e.Asunto = PedirTexto("Asunto o descripción: ");
            e.FechaRegistro = DateTime.Now;
            expedientes.Add(e);
            GuardarDatos();

            Console.WriteLine("\nDocumento registrado correctamente.");
            Console.WriteLine("Código asignado: " + e.Codigo);
            Pausa();
        }

        private static void BuscarDocumento()
        {
            Console.Clear();
            Titulo("BUSCAR DOCUMENTO");
            Console.Write("Ingrese código, DNI o nombre del remitente: ");
            string dato = (Console.ReadLine() ?? "").Trim();

            var encontrados = expedientes.Where(e =>
                e.Codigo.Contains(dato, StringComparison.OrdinalIgnoreCase) ||
                e.Dni.Contains(dato, StringComparison.OrdinalIgnoreCase) ||
                e.Remitente.Contains(dato, StringComparison.OrdinalIgnoreCase)).ToList();

            if (encontrados.Count == 0) Console.WriteLine("No se encontraron documentos.");
            else foreach (var e in encontrados) MostrarExpediente(e);
            Pausa();
        }

        private static void MostrarDocumentos()
        {
            Console.Clear();
            Titulo("LISTA DE DOCUMENTOS");
            if (expedientes.Count == 0) Console.WriteLine("Todavía no hay documentos registrados.");
            else foreach (var e in expedientes) MostrarExpediente(e);
            Pausa();
        }

        private static void OrdenarDocumentos()
        {
            Console.Clear();
            Titulo("ORDENAR DOCUMENTOS");
            Console.WriteLine("1. Por fecha (más recientes primero)");
            Console.WriteLine("2. Por nombre del remitente");
            Console.Write("Seleccione: ");
            string opcion = (Console.ReadLine() ?? "").Trim();

            if (opcion == "1")
                expedientes = expedientes.OrderByDescending(e => e.FechaRegistro).ToList();
            else if (opcion == "2")
                expedientes = expedientes.OrderBy(e => e.Remitente).ToList();
            else
            {
                Console.WriteLine("Opción no válida.");
                Pausa();
                return;
            }

            GuardarDatos();
            Console.WriteLine("\nDocumentos ordenados correctamente.");
            foreach (var e in expedientes) Console.WriteLine($"{e.Codigo} | {e.Remitente} | {e.FechaRegistro:dd/MM/yyyy HH:mm}");
            Pausa();
        }

        private static void AgendarCita()
        {
            Console.Clear();
            Titulo("AGENDAR CITA");
            Cita c = new Cita();
            c.Codigo = "CIT-" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            c.Paciente = PedirTexto("Nombre completo del paciente: ");
            c.Dni = PedirDni();
            c.Telefono = PedirTelefono();

            DateTime fecha;
            while (true)
            {
                Console.Write("Fecha de la cita (dd/MM/yyyy): ");
                if (DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out fecha) && fecha.Date >= DateTime.Today)
                    break;
                Console.WriteLine("Fecha inválida. No puede ser anterior al día de hoy.");
            }
            c.Fecha = fecha.Date;

            c.HoraInicio = PedirHora("Hora de inicio (HH:mm): ");
            c.HoraFin = PedirHora("Hora de término (HH:mm): ");

            if (c.HoraFin <= c.HoraInicio)
            {
                Console.WriteLine("La hora de término debe ser posterior a la de inicio.");
                Pausa();
                return;
            }

            if (c.HoraInicio < new TimeSpan(8, 0, 0) || c.HoraFin > new TimeSpan(17, 0, 0))
            {
                Console.WriteLine("El horario de atención es de 08:00 a 17:00.");
                Pausa();
                return;
            }

            if ((c.HoraFin - c.HoraInicio).TotalMinutes < 15)
            {
                Console.WriteLine("La cita debe durar como mínimo 15 minutos.");
                Pausa();
                return;
            }

            if (c.Fecha == DateTime.Today && c.HoraInicio <= DateTime.Now.TimeOfDay)
            {
                Console.WriteLine("La hora de inicio debe ser posterior a la hora actual.");
                Pausa();
                return;
            }

            bool cruza = citas.Any(x => x.Estado == "Activa" && x.Fecha.Date == c.Fecha.Date &&
                c.HoraInicio < x.HoraFin && c.HoraFin > x.HoraInicio);
            if (cruza)
            {
                Console.WriteLine("Ese horario se cruza con otra cita activa. Elija otro horario.");
                Pausa();
                return;
            }

            c.Motivo = PedirTexto("Motivo de la cita: ");
            c.Estado = "Activa";

            string contenido = CrearComprobante(c);
            string ruta = CrearArchivoUnico(c.Codigo, contenido);
            c.ArchivoComprobante = ruta;

            citas.Add(c);
            GuardarDatos();

            Console.WriteLine("\nCita registrada correctamente.");
            Console.WriteLine("Código: " + c.Codigo);
            Console.WriteLine("Comprobante: " + ruta);
            AbrirEnBlocDeNotas(ruta);
            Pausa();
        }

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

        private static string CrearArchivoUnico(string codigo, string contenido)
        {
            Directory.CreateDirectory(CarpetaComprobantes);
            string baseNombre = "Cita_" + codigo;
            string ruta = Path.Combine(CarpetaComprobantes, baseNombre + ".txt");
            int n = 1;

            while (true)
            {
                try
                {
                    using (FileStream fs = new FileStream(ruta, FileMode.CreateNew, FileAccess.Write))
                    using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                        sw.Write(contenido);
                    return ruta;
                }
                catch (IOException)
                {
                    ruta = Path.Combine(CarpetaComprobantes, baseNombre + "_" + n + ".txt");
                    n++;
                }
            }
        }

        private static void AbrirEnBlocDeNotas(string ruta)
        {
            try
            {
                Process.Start(new ProcessStartInfo("notepad.exe", "\"" + ruta + "\"") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("No se pudo abrir el Bloc de notas automáticamente: " + ex.Message);
            }
        }

        private static void VerAgenda()
        {
            Console.Clear();
            Titulo("AGENDA DE CITAS");
            var lista = citas.OrderBy(c => c.Fecha).ThenBy(c => c.HoraInicio).ToList();
            if (lista.Count == 0) Console.WriteLine("No hay citas registradas.");
            foreach (var c in lista) MostrarCita(c);
            Pausa();
        }

        private static void BuscarCita()
        {
            Console.Clear();
            Titulo("BUSCAR CITA");
            Console.Write("Ingrese código, DNI o nombre del paciente: ");
            string dato = (Console.ReadLine() ?? "").Trim();

            var encontrados = citas.Where(c =>
                c.Codigo.Contains(dato, StringComparison.OrdinalIgnoreCase) ||
                c.Dni.Contains(dato, StringComparison.OrdinalIgnoreCase) ||
                c.Paciente.Contains(dato, StringComparison.OrdinalIgnoreCase)).ToList();

            if (encontrados.Count == 0) Console.WriteLine("No se encontraron citas.");
            else foreach (var c in encontrados) MostrarCita(c);
            Pausa();
        }

        private static void CancelarCita()
        {
            Console.Clear();
            Titulo("CANCELAR CITA");
            Console.Write("Ingrese el código de la cita: ");
            string codigo = (Console.ReadLine() ?? "").Trim();
            Cita? cita = citas.FirstOrDefault(c => c.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));

            if (cita == null)
                Console.WriteLine("No existe una cita con ese código.");
            else if (cita.Estado != "Activa")
                Console.WriteLine("La cita ya se encuentra " + cita.Estado.ToLower() + ".");
            else
            {
                cita.Estado = "Cancelada";
                GuardarDatos();
                Console.WriteLine("Cita cancelada correctamente. El comprobante original se conserva.");
            }
            Pausa();
        }

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
            Console.WriteLine($"Comprobante: {c.ArchivoComprobante}");
        }

        private static string PedirTexto(string mensaje)
        {
            while (true)
            {
                Console.Write(mensaje);
                string valor = (Console.ReadLine() ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(valor)) return valor;
                Console.WriteLine("Este campo no puede quedar vacío.");
            }
        }

        private static string PedirDni()
        {
            while (true)
            {
                Console.Write("DNI (8 dígitos): ");
                string dni = (Console.ReadLine() ?? "").Trim();
                if (dni.Length == 8 && dni.All(char.IsDigit)) return dni;
                Console.WriteLine("El DNI debe contener exactamente 8 números.");
            }
        }

        private static string PedirTelefono()
        {
            while (true)
            {
                Console.Write("Teléfono (9 dígitos): ");
                string telefono = (Console.ReadLine() ?? "").Trim();
                if (telefono.Length == 9 && telefono.All(char.IsDigit)) return telefono;
                Console.WriteLine("El teléfono debe contener exactamente 9 números.");
            }
        }

        private static TimeSpan PedirHora(string mensaje)
        {
            while (true)
            {
                Console.Write(mensaje);
                if (TimeSpan.TryParseExact(Console.ReadLine(), @"hh\:mm", CultureInfo.InvariantCulture, out TimeSpan hora))
                    return hora;
                Console.WriteLine("Formato incorrecto. Use HH:mm, por ejemplo 09:30.");
            }
        }

        private static void Titulo(string texto)
        {
            Console.WriteLine("==============================================================");
            Console.WriteLine(" " + texto);
            Console.WriteLine("==============================================================\n");
        }

        private static void Pausa()
        {
            Console.WriteLine("\nPresione ENTER para continuar...");
            Console.ReadLine();
        }
    }
}