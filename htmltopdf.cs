using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public static class htmlTopdf
    {
        public static Task<FileStreamResult> HtmlToPdf(string html, IWebHostEnvironment env)
        {
            byte[] pdf = WkHtmltoPdf(html, env);
            var stream = new MemoryStream(pdf);
            var fileStreamResult = new FileStreamResult(stream, new MediaTypeHeaderValue("application/pdf"));
            return Task.FromResult(fileStreamResult);
        }
        private static byte[] WkHtmltoPdf(string html, IWebHostEnvironment env)
        {
            string switches = "-q -s A4 -O Landscape -";
            if (!string.IsNullOrEmpty(html))
            {
                switches += " -";
                html = SpecialCharsEncode(html);
            }
            using (var proc = new Process())
            {
                proc.StartInfo = new ProcessStartInfo
                {

                    FileName = Path.Combine(env.WebRootPath, "external-lib\\wkhtmltopdf.exe"), //"wkhtmltopdf.exe",                   
                    Arguments = switches,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                };
                proc.Start();
                if (!string.IsNullOrEmpty(html))
                {
                    using (var sIn = proc.StandardInput)
                    {
                        sIn.WriteLine(html);
                    }
                }
                using (var ms = new MemoryStream())
                {
                    using (var sOut = proc.StandardOutput.BaseStream)
                    {
                        byte[] buffer = new byte[4096];
                        int read;
                        while ((read = sOut.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, read);
                        }
                    }
                    string error = proc.StandardError.ReadToEnd();
                    if (ms.Length == 0)
                    {
                        throw new Exception(error);
                    }
                    proc.WaitForExit();
                    return ms.ToArray();
                }
            }
        }
        private static string SpecialCharsEncode(string text)
        {
            var chars = text.ToCharArray();
            var result = new StringBuilder(text.Length + (int)(text.Length * 0.1));

            foreach (var c in chars)
            {
                var value = System.Convert.ToInt32(c);
                if (value > 127)
                    result.AppendFormat("&#{0};", value);
                else
                    result.Append(c);
            }

            return result.ToString();
        }
    }
}
