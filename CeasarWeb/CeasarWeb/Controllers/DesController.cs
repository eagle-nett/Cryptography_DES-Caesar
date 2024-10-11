using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using CeasarWeb.Models; // Namespace chứa DbContext và model DuLieuDes

namespace CeasarWeb.Controllers
{
    public class DesController : Controller
    {
        // Sử dụng DbContext đã được tạo từ Database First
        private readonly DataEntities2 _context = new DataEntities2();

        // GET: Des
        public ActionResult Index()
        {
            return View();
        }

        // POST: Des/Process
        [HttpPost]
        public ActionResult Process(string plainTextHex, string keyHex, string actionType)
        {
            if (keyHex.Length != 16) // Khóa phải có độ dài 16 ký tự hex (8 byte)
            {
                ModelState.AddModelError("Key", "Khóa phải có độ dài 16 ký tự (8 byte).");
                return View("Index");
            }

            byte[] key = ConvertHexStringToByteArray(keyHex);
            string result;

            if (actionType == "Encrypt")
            {
                byte[] plainText = ConvertHexStringToByteArray(plainTextHex);
                result = EncryptByteArray(plainText, key);
            }
            else // actionType == "Decrypt"
            {
                byte[] cipherText = ConvertHexStringToByteArray(plainTextHex);
                result = DecryptByteArray(cipherText, key);
            }

            // Lưu dữ liệu vào bảng DuLieuDes trong database
            DuLieuDes duLieuDes = new DuLieuDes
            {
                DuLieu = result,
                Khoa = keyHex
            };
            _context.DuLieuDes.Add(duLieuDes);
            _context.SaveChanges(); // Lưu thay đổi vào database

            ViewBag.Result = result;
            return View("Index");
        }

        private string EncryptByteArray(byte[] plainText, byte[] key)
        {
            using (var des = DES.Create())
            {
                des.Key = key;
                des.IV = new byte[8]; // IV là 0
                des.Padding = PaddingMode.None; // Không sử dụng padding

                using (var encryptor = des.CreateEncryptor(des.Key, des.IV))
                using (var ms = new MemoryStream())
                {
                    // Đảm bảo độ dài dữ liệu là bội số của 8
                    int padBytes = 8 - (plainText.Length % 8);
                    if (padBytes < 8)
                    {
                        Array.Resize(ref plainText, plainText.Length + padBytes);
                    }

                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(plainText, 0, plainText.Length);
                        cs.FlushFinalBlock();
                    }

                    return BitConverter.ToString(ms.ToArray()).Replace("-", "").ToUpper();
                }
            }
        }

        private string DecryptByteArray(byte[] cipherText, byte[] key)
        {
            using (var des = DES.Create())
            {
                des.Key = key;
                des.IV = new byte[8]; // IV là 0
                des.Padding = PaddingMode.None; // Không sử dụng padding

                using (var decryptor = des.CreateDecryptor(des.Key, des.IV))
                using (var ms = new MemoryStream(cipherText))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var resultStream = new MemoryStream())
                {
                    cs.CopyTo(resultStream);
                    byte[] decryptedBytes = resultStream.ToArray();

                    // Chuyển đổi byte array thành hex string
                    return BitConverter.ToString(decryptedBytes).Replace("-", "").ToUpper();
                }
            }
        }

        private byte[] ConvertHexStringToByteArray(string hex)
        {
            int length = hex.Length;
            byte[] bytes = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
    }
}
