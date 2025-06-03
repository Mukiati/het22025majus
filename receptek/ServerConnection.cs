using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace receptek
{
    public class ServerConnection
    {
        public readonly HttpClient client = new HttpClient();
        public readonly string serverUrl;

        public ServerConnection(string serverUrl)
        {
            this.serverUrl = serverUrl;
        }

        public async Task<bool> Login(string user, string pass)
        {
            string url = serverUrl + "/auth/login";
            try
            {
                var jsonInfo = new { username = user, password = pass };
                string json = JsonConvert.SerializeObject(jsonInfo);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                string result = await response.Content.ReadAsStringAsync();
                JsonData data = JsonConvert.DeserializeObject<JsonData>(result);
                Token.token = data.token;
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Hiba a bejelentkezéskor: {e.Message}");
                return false;
            }
        }

        public async Task<bool> Reg(string user, string pass)
        {
            string url = serverUrl + "/users/register";
            try
            {
                var jsonInfo = new { username = user, password = pass };
                string json = JsonConvert.SerializeObject(jsonInfo);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Hiba a regisztrációkor: {e.Message}");
                return false;
            }
        }

        public async Task<JsonData> GetProfile()
        {
            string url = serverUrl + "/users/me";
            try
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token.token);

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<JsonData>(result);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Hiba a profil betöltésekor: {e.Message}");
                return null;
            }
        }

        public async Task<bool> CreateRecipe(string name, string ingredients, int? categoryId = null)
        {
            string url = serverUrl + "/recipes";
            try
            {
                var jsonInfo = new { name, ingredients, categoryId };
                string json = JsonConvert.SerializeObject(jsonInfo);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token.token);

                HttpResponseMessage response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Hiba a recept létrehozásakor: {e.Message}");
                return false;
            }
        }
        public string hiba = "";
        public async Task<List<ReceptData>> GetAllRecipes()
        {
            string url = serverUrl + "/recipes";
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ReceptData>>(result);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Hiba a receptek betöltésekor: {e.Message}");
                hiba = e.Message;

                return null;
            }
        }

        public async Task<bool> DeleteRecipe(int recipeId)
        {
            string url = $"{serverUrl}/recipes/{recipeId}";
            try
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token.token);

                HttpResponseMessage response = await client.DeleteAsync(url);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Hiba a törlés során: {e.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateRecipe(int recipeId, string name, string ingredients, int? categoryId = null)
        {
            string url = $"{serverUrl}/recipes/{recipeId}";
            try
            {
                var jsonInfo = new { name, ingredients, categoryId };
                string json = JsonConvert.SerializeObject(jsonInfo);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token.token);

                HttpResponseMessage response = await client.PutAsync(url, content);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a frissítéskor: {ex.Message}");
                return false;
            }
        }

        public async Task<List<ReceptData>> SearchRecipes(string title)
        {
            string url = $"{serverUrl}/recipes/title/{Uri.EscapeDataString(title)}";
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ReceptData>>(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a kereséskor: {ex.Message}");
                return null;
            }
        }


        public async Task<bool> CreateCategory(string name)
        {
            string url = serverUrl + "/categories";
            try
            {
                var jsonInfo = new { name };
                string json = JsonConvert.SerializeObject(jsonInfo);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token.token);

                HttpResponseMessage response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Hiba a kategória létrehozásakor: {e.Message}");
                return false;
            }
        }

        public async Task<List<CategoryData>> GetAllCategories()
        {
            string url = serverUrl + "/categories";
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<CategoryData>>(result);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Hiba a kategóriák betöltésekor: {e.Message}");
                return null;
            }
        }

        public async Task<bool> AssignCategoryToRecipe(int recipeId, int categoryId)
        {
            string url = $"{serverUrl}/recipes/{recipeId}/category";
            try
            {
                // <- EZT JAVÍTOTTUK vissza categoryId-re!
                var jsonInfo = new { categoryId = categoryId };
                string json = JsonConvert.SerializeObject(jsonInfo);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token.token);

                HttpResponseMessage response = await client.PutAsync(url, content);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Sikertelen hozzárendelés: {(int)response.StatusCode} - {response.ReasonPhrase}\nRészletek: {responseContent}");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Hiba a kategória hozzárendelésekor: {e.Message}");
                return false;
            }
        }


        public async Task<List<ReceptData>> SearchRecipesByCategory(string categoryName)
        {
            string url = $"{serverUrl}/recipes?category={Uri.EscapeDataString(categoryName)}";
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var recipes = JsonConvert.DeserializeObject<List<ReceptData>>(content);
                return recipes;
            }
            return new List<ReceptData>();
        }

        public async Task<List<ReceptData>> SearchRecipesByIngredient(string ingredient)
        {
            string url = $"{serverUrl}/recipes?ingredient={Uri.EscapeDataString(ingredient)}";
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ReceptData>>(result);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Hiba a hozzávaló alapú kereséskor: {e.Message}");
                return null;
            }
        }

        public async Task<List<PopularCategoryData>> GetPopularCategories()
        {
            string url = serverUrl + "/categories/popular";
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<PopularCategoryData>>(result);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Hiba a népszerű kategóriák betöltésekor: {e.Message}");
                return null;
            }
        }
    }
}