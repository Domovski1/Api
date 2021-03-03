using API.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace API
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpListener server = new HttpListener();
            Console.WriteLine("Сервер работает...");
            // Прописываем адресс, к которому будем обращаться
            server.Prefixes.Add("http://localhost:2117/");
            server.Start();

            // Нужен для декодировки текста
            JsonSerializerOptions options = new JsonSerializerOptions { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };

            while(server.IsListening)
            {
                HttpListenerContext context = server.GetContext();
                if (context.Request.HttpMethod == "GET")
                {
                    try
                    {
                        // Может задаваться произвольно
                        if (context.Request.RawUrl == "/movie/")
                        {
                            var FilmList = BaseConnect.db.Film.ToList();
                            string response = JsonSerializer.Serialize<List<Film>>(FilmList, options);

                            byte[] data = Encoding.UTF8.GetBytes(response);
                            context.Response.ContentType = "application/json;charset=utf-8";
                            using (Stream stream = context.Response.OutputStream)
                            {
                                stream.Write(data, 0, data.Length);

                            }
                            context.Response.StatusCode = 200;
                            context.Response.Close();
                        } else
                        {
                            throw new Exception();
                        }
                    }
                    catch
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }



                if (context.Request.HttpMethod == "POST")
                {
                    try
                    {
                        if (context.Request.RawUrl == "/movie/")
                        {
                            string response = "";
                            byte[] data = new byte[context.Request.ContentLength64];

                            using (Stream stream = context.Request.InputStream)
                            {
                                stream.Read(data, 0, data.Length);
                            }

                            response = UTF8Encoding.UTF8.GetString(data);
                            var CurrentMovie = JsonSerializer.Deserialize<List<Film>>(response);

                            foreach (var item in CurrentMovie)
                            {
                                var movie = BaseConnect.db.Film.FirstOrDefault(x => x.Title == item.Title);
                                if (movie != null)
                                {
                                    throw new Exception();
                                }
                            }

                            BaseConnect.db.Film.AddRange(CurrentMovie);
                            BaseConnect.db.SaveChanges();
                            context.Response.StatusCode = 200;
                            context.Response.Close();

                        } else
                        {
                            throw new Exception();
                        }
                    }
                    catch (Exception ex)
                    {

                        context.Response.StatusCode = 400;
                        context.Response.Close();
                        throw new Exception();
                    }
                }
            }



        }
    }
}
