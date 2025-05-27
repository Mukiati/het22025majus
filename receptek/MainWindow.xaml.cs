using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http;
using Newtonsoft.Json;

namespace receptek
{
    public partial class MainWindow : Window
    {
        ServerConnection connection;
        public MainWindow()
        {
            InitializeComponent();
            Start();
        }

        async void Start()
        {
            connection = new ServerConnection("http://localhost:33333");
            await LoadRecipes();
            await LoadCategories();
        }

        // Bejelentkezés
        async void LoginClick(object s, EventArgs e)
        {
            bool success = await connection.Login(nameinput.Text, passwordinput.Password);
            if (success)
            {
                MessageBox.Show("Sikeres bejelentkezés!");
            }
        }

        // Regisztráció
        async void RegClick(object s, EventArgs e)
        {
            bool success = await connection.Reg(nameinput.Text, passwordinput.Password);
            if (success)
            {
                MessageBox.Show("Sikeres regisztráció!");
            }
        }

        // Profil mutatása
        async void ShowProfileClick(object s, EventArgs e)
        {
            if (string.IsNullOrEmpty(Token.token))
            {
                MessageBox.Show("Először jelentkezz be!");
                return;
            }

            try
            {
                JsonData profile = await connection.GetProfile();
                if (profile != null)
                {
                    profilmutatasgeci.Content =
                        $"Felhasználónév: {profile.username}\n" +
                        $"Regisztrálva: {profile.createdAt:yyyy.MM.dd}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba: {ex.Message}");
            }
        }

        // Recept létrehozása
        async void CreateRecipeClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(recipeNameInput.Text) || string.IsNullOrEmpty(recipeIngredientsInput.Text))
            {
                MessageBox.Show("Töltsd ki mindkét mezőt!");
                return;
            }

            var selectedCategory = (CategoryData)categoryComboBox.SelectedItem;
            int? categoryId = selectedCategory?.id;

            bool success = await connection.CreateRecipe(recipeNameInput.Text, recipeIngredientsInput.Text, categoryId);
            if (success)
            {
                MessageBox.Show("Recept sikeresen mentve!");
                recipeNameInput.Clear();
                recipeIngredientsInput.Clear();
                await LoadRecipes();
            }
        }

        private async void LoadRecipesClick(object sender, EventArgs e)
        {
            await LoadRecipes();
        }

        private async Task LoadRecipes()
        {
            var recipes = await connection.GetAllRecipes();
            if (recipes != null)
            {
                recipesWrapPanel.Children.Clear();

                foreach (var recipe in recipes)
                {
                    var stackPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(5)
                    };

                    var nameBlock = new TextBlock
                    {
                        Text = recipe.name,
                        Width = 150,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    var detailsButton = new Button
                    {
                        Content = "Részletek",
                        Margin = new Thickness(5),
                        Tag = recipe.id
                    };
                    detailsButton.Click += ShowDetails_Click;

                    var deleteButton = new Button
                    {
                        Content = "Törlés",
                        Margin = new Thickness(5),
                        Tag = recipe.id
                    };
                    deleteButton.Click += DeleteRecipe_Click;

                    stackPanel.Children.Add(nameBlock);
                    stackPanel.Children.Add(detailsButton);
                    stackPanel.Children.Add(deleteButton);
                    recipesWrapPanel.Children.Add(stackPanel);
                }
            }
        }

        // Részletek gomb
        private async void ShowDetails_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int recipeId = (int)button.Tag;

            try
            {
                var response = await connection.client.GetAsync($"{connection.serverUrl}/recipes/{recipeId}");
                response.EnsureSuccessStatusCode();
                var recipe = JsonConvert.DeserializeObject<ReceptData>(await response.Content.ReadAsStringAsync());
                MessageBox.Show($"ID: {recipe.id}\nNév: {recipe.name}\nÖsszetevők: {recipe.ingredients}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba: {ex.Message}");
            }
        }

        private async void DeleteRecipe_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int recipeId = (int)button.Tag;

            bool success = await connection.DeleteRecipe(recipeId);
            if (success)
            {
                await LoadRecipes();
                MessageBox.Show("Recept sikeresen törölve!");
            }
        }

        // Recept szerkesztése
        private async void EditRecipeClick(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(editIdInput.Text, out int recipeId))
            {
                MessageBox.Show("Érvénytelen ID formátum!");
                return;
            }

            if (string.IsNullOrEmpty(editNameInput.Text) || string.IsNullOrEmpty(editIngredientsInput.Text))
            {
                MessageBox.Show("Töltsd ki mindkét mezőt!");
                return;
            }


            bool success = await connection.UpdateRecipe(
                recipeId,
                editNameInput.Text,
                editIngredientsInput.Text
            );

            if (success)
            {
                MessageBox.Show("Recept sikeresen frissítve!");
                editIdInput.Clear();
                editNameInput.Clear();
                editIngredientsInput.Clear();
                await LoadRecipes();
            }
        }

        // Keresés név alapján
        private async void SearchRecipeClick(object sender, RoutedEventArgs e)
        {
            string query = searchInput.Text;
            if (string.IsNullOrWhiteSpace(query))
            {
                MessageBox.Show("Írj be egy nevet a kereséshez!");
                return;
            }

            var results = await connection.SearchRecipes(query);
            DisplaySearchResults(results);
        }

        // Keresés hozzávaló alapján
        private async void SearchByIngredientClick(object sender, RoutedEventArgs e)
        {
            string query = searchInput.Text;
            if (string.IsNullOrWhiteSpace(query))
            {
                MessageBox.Show("Írj be egy hozzávalót a kereséshez!");
                return;
            }

            var results = await connection.SearchRecipesByIngredient(query);
            DisplaySearchResults(results);
        }

        // Keresés kategória alapján
        private async void SearchByCategoryClick(object sender, RoutedEventArgs e)
        {
            var selectedCategory = (CategoryData)categoryFilterComboBox.SelectedItem;
            if (selectedCategory == null)
            {
                MessageBox.Show("Válassz ki egy kategóriát!");
                return;
            }

            var results = await connection.SearchRecipesByCategory(selectedCategory.id);
            DisplaySearchResults(results);
        }

        private void DisplaySearchResults(List<ReceptData> results)
        {
            searchResultsPanel.Children.Clear();

            if (results != null && results.Any())
            {
                foreach (var recipe in results)
                {
                    var stackPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(5)
                    };

                    var nameBlock = new TextBlock
                    {
                        Text = recipe.name,
                        Width = 120,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    var detailsButton = new Button
                    {
                        Content = "Részletek",
                        Margin = new Thickness(5),
                        Tag = recipe.id
                    };
                    detailsButton.Click += ShowDetails_Click;

                    var deleteButton = new Button
                    {
                        Content = "Törlés",
                        Margin = new Thickness(5),
                        Tag = recipe.id
                    };
                    deleteButton.Click += DeleteRecipe_Click;

                    stackPanel.Children.Add(nameBlock);
                    stackPanel.Children.Add(detailsButton);
                    stackPanel.Children.Add(deleteButton);
                    searchResultsPanel.Children.Add(stackPanel);
                }
            }
            else
            {
                searchResultsPanel.Children.Add(new TextBlock { Text = "Nincs találat!" });
            }
        }

        // Hozzávaló hozzáadása
        public async void AddIngredientToRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(ingredientAddRecipeId.Text, out int recipeId) || string.IsNullOrWhiteSpace(ingredientToAdd.Text))
            {
                MessageBox.Show("Adj meg érvényes recept ID-t és hozzávalót!");
                return;
            }

            var response = await connection.client.GetAsync($"{connection.serverUrl}/recipes/{recipeId}");
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("Nem található ilyen recept.");
                return;
            }

            var recipe = JsonConvert.DeserializeObject<ReceptData>(await response.Content.ReadAsStringAsync());

            var ingredientsList = recipe.ingredients.Split(',').Select(x => x.Trim()).ToList();
            ingredientsList.Add(ingredientToAdd.Text.Trim());
            string updatedIngredients = string.Join(", ", ingredientsList);

            var payload = new { name = recipe.name, ingredients = updatedIngredients };
            var patchResponse = await connection.client.PutAsync(
                $"{connection.serverUrl}/recipes/{recipeId}",
                new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            );

            if (patchResponse.IsSuccessStatusCode)
            {
                MessageBox.Show("Hozzávaló hozzáadva.");
                ingredientToAdd.Clear();
            }
            else
            {
                MessageBox.Show("Nem sikerült hozzáadni.");
            }
        }

        // Hozzávalók megtekintése
        public async void ViewIngredients_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(ingredientViewRecipeId.Text, out int recipeId))
            {
                MessageBox.Show("Adj meg érvényes recept ID-t!");
                return;
            }

            var response = await connection.client.GetAsync($"{connection.serverUrl}/recipes/{recipeId}");
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("Recept nem található.");
                return;
            }

            var recipe = JsonConvert.DeserializeObject<ReceptData>(await response.Content.ReadAsStringAsync());
            var ingredients = recipe.ingredients.Split(',').Select(x => x.Trim()).ToList();

            ingredientsListBox.Items.Clear();

            foreach (var ingredient in ingredients)
            {
                var stack = new StackPanel { Orientation = Orientation.Horizontal };
                stack.Children.Add(new TextBlock { Text = ingredient, Width = 150 });

                var deleteButton = new Button
                {
                    Content = "Törlés",
                    Tag = new Tuple<int, string>(recipeId, ingredient),
                    Margin = new Thickness(5, 0, 0, 0)
                };
                deleteButton.Click += DeleteIngredientFromRecipe_Click;

                stack.Children.Add(deleteButton);
                ingredientsListBox.Items.Add(stack);
            }
        }

        // Hozzávaló törlése
        public async void DeleteIngredientFromRecipe_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var tag = (Tuple<int, string>)button.Tag;
            int recipeId = tag.Item1;
            string ingredientToRemove = tag.Item2;

            var response = await connection.client.GetAsync($"{connection.serverUrl}/recipes/{recipeId}");
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("Nem található recept.");
                return;
            }

            var recipe = JsonConvert.DeserializeObject<ReceptData>(await response.Content.ReadAsStringAsync());
            var ingredientsList = recipe.ingredients.Split(',').Select(x => x.Trim()).ToList();

            ingredientsList.RemoveAll(x => x.Equals(ingredientToRemove, StringComparison.OrdinalIgnoreCase));
            string updatedIngredients = string.Join(", ", ingredientsList);

            var payload = new { name = recipe.name, ingredients = updatedIngredients };
            var putResponse = await connection.client.PutAsync(
                $"{connection.serverUrl}/recipes/{recipeId}",
                new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            );

            if (putResponse.IsSuccessStatusCode)
            {
                MessageBox.Show("Hozzávaló törölve.");
                ViewIngredients_Click(null, null);
            }
            else
            {
                MessageBox.Show("Törlés sikertelen.");
            }
        }

        // Kategória létrehozása
        private async void CreateCategoryClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(newCategoryName.Text))
            {
                MessageBox.Show("Írd be a kategória nevét!");
                return;
            }

            bool success = await connection.CreateCategory(newCategoryName.Text);
            if (success)
            {
                MessageBox.Show("Kategória sikeresen létrehozva!");
                newCategoryName.Clear();
                await LoadCategories();
            }
        }

        // Kategóriák betöltése
        private async void LoadCategoriesClick(object sender, RoutedEventArgs e)
        {
            await LoadCategories();
        }

        private async Task LoadCategories()
        {
            var categories = await connection.GetAllCategories();
            if (categories != null)
            {
                categoriesListBox.ItemsSource = categories;
                categoryComboBox.ItemsSource = categories;
                categoryFilterComboBox.ItemsSource = categories;
                assignCategoryComboBox.ItemsSource = categories;
            }
        }

        // Kategória hozzárendelése recepthez
        private async void AssignCategoryClick(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(recipeIdForCategory.Text, out int recipeId))
            {
                MessageBox.Show("Érvénytelen recept ID!");
                return;
            }

            var selectedCategory = (CategoryData)assignCategoryComboBox.SelectedItem;
            if (selectedCategory == null)
            {
                MessageBox.Show("Válassz ki egy kategóriát!");
                return;
            }

            bool success = await connection.AssignCategoryToRecipe(recipeId, selectedCategory.id);
            if (success)
            {
                MessageBox.Show("Kategória sikeresen hozzárendelve!");
                recipeIdForCategory.Clear();
                await LoadRecipes();
            }
        }

        // Legnépszerűbb kategóriák betöltése
        private async void LoadPopularCategoriesClick(object sender, RoutedEventArgs e)
        {
            var popularCategories = await connection.GetPopularCategories();
            if (popularCategories != null)
            {
                popularCategoriesListView.ItemsSource = popularCategories;
            }
        }
    }

    public class CategoryData
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class PopularCategoryData
    {
        public CategoryData category { get; set; }
        public int count { get; set; }
    }
}