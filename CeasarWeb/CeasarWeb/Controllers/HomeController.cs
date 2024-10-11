using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CeasarWeb.Models;

namespace CeasarWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataEntities2 _context = new DataEntities2(); 

        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        // POST: Mã hóa và giải mã
        [HttpPost]
        public ActionResult Index(string message, int key, string actionType)
        {
            string result = string.Empty;
            List<char> originalChars = message.ToList();
            List<char> transformedChars = new List<char>();

            if (actionType == "Encrypt")
            {
                result = CaesarEncrypt(message, key);
                transformedChars = result.ToList();
            }
            else if (actionType == "Decrypt")
            {
                result = CaesarDecrypt(message, key);
                transformedChars = result.ToList();
            }

            // Lưu dữ liệu vào database
            DuLieu duLieu = new DuLieu
            {
                BanRo = message,
                Khoa = key.ToString()
            };
            _context.DuLieu.Add(duLieu);
            _context.SaveChanges();

            ViewBag.Result = result;
            ViewBag.OriginalChars = originalChars;
            ViewBag.TransformedChars = transformedChars;
            return View();
        }

        private string CaesarEncrypt(string message, int key)
        {
            char[] buffer = message.ToCharArray();

            for (int i = 0; i < buffer.Length; i++)
            {
                char letter = buffer[i];

                if (char.IsLetter(letter))
                {
                    char offset = char.IsUpper(letter) ? 'A' : 'a';
                    letter = (char)(((letter + key - offset) % 26) + offset);
                }

                buffer[i] = letter;
            }

            return new string(buffer);
        }

        private string CaesarDecrypt(string message, int key)
        {
            return CaesarEncrypt(message, 26 - key);
        }
    }
}
