using Frontend.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using Frontend.Helper;
using NuGet.Packaging.Signing;

namespace Frontend.Controllers
{
    public class AuthenticationController : Controller
    {


        public AuthenticationController()
        {

        }
        public async Task<IActionResult> Index()
        {
            return View();
        }
        [HttpPost]

        public async Task<IActionResult> Login(UserLoginDto user)
        {
            HttpClient client = new HttpClient();
            // Token almak için API'ye istek atıyoruz
            HttpResponseMessage responseMessage = await client.PostAsJsonAsync("http://localhost:5251/api/Authentication/login", user);

            if (responseMessage.IsSuccessStatusCode)
            {
                // Token verisini alıyoruz
                var data = await responseMessage.Content.ReadAsStringAsync();
                TokenDTO tokenDTO = JsonConvert.DeserializeObject<TokenDTO>(data);

                if (tokenDTO != null)
                {
                    // Authorization başlığını ekliyoruz
                    GenericClient.Client.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokenDTO.AccessToken);

                    TokenStaticDto.AccessToken = tokenDTO.AccessToken;
                    TokenStaticDto.RefreshToken = tokenDTO.RefreshToken;

                    // Claims verilerini almak için API çağrısı
                    string myUrl = $"http://localhost:5251/api/Authentication/get-cliams?userName={user.UserName}";
                    HttpResponseMessage claimResponse = await GenericClient.Client.GetAsync(myUrl);
                    var myClaimData = await claimResponse.Content.ReadFromJsonAsync<List<ClaimDto>>();

                    if (myClaimData != null)
                    {
                        // Claims'leri alıp ClaimsIdentity oluşturuyoruz
                        //  var claims = myClaimData.Select(c => new Claim(c.Type, c.Value)).ToList();
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true // Oturum kalıcı olacak
                        };

                        var claims = myClaimData.Select(c => new Claim(c.Type, c.Value)).ToList();
                        var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(userIdentity),
                            authProperties);

                        Console.WriteLine("Claims Count: " + claims.Count);
                        var currentClaims = HttpContext.User.Claims.ToList();
                        Console.WriteLine("Current Claims Count: " + currentClaims.Count);
                    }
                    else
                    {
                        // Claim verileri alınamadıysa hata veriyoruz
                        ModelState.AddModelError("", "Claim verileri alınamadı.");
                        return View("Index");
                    }
                }

                // Başarılı ise, kullanıcıyı anasayfaya yönlendiriyoruz
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Kullanıcı adı veya şifre yanlış.");
                return RedirectToAction("Index", "Authentication");
            }
        }





        public async Task<IActionResult> LogOut()
        {
            if (GenericClient.Client.DefaultRequestHeaders.Contains("Authorization"))
            {
                GenericClient.Client.DefaultRequestHeaders.Remove("Authorization");
            }
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Redirect to the login page
            return RedirectToAction("Index", "Authentication");
        }

    }
}
