using AzureStorageLibrary;
using AzureStorageLibrary.Models;
using AzureStorageLibrary.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using MVCWebApp.Models;
using System.Diagnostics;
using System.Net;

namespace MVCWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly INoSqlStorage<Product> _noSqlStorage;
        private readonly INoSqlStorage<Store> _noSqlStore;

        public HomeController(INoSqlStorage<Product> noSqlStorage, INoSqlStorage<Store> noSqlStore)
        {
            _noSqlStorage = noSqlStorage;
            _noSqlStore = noSqlStore;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.products = (await _noSqlStorage.All()).ToList();
            ViewBag.stores = (await _noSqlStore.All()).ToList();
            ViewBag.IsUpdate = false;
            var model = new ProductAndStoreViewModel
            {
                Product = new Product(),
                Store = new Store(),
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddStore(ProductAndStoreViewModel model)
        {
            var stor = new Store
            {
                Timestamp = model.Store.Timestamp,
                RowKey = model.Store.RowKey,
                PartitionKey = model.Store.PartitionKey,
                ETag = model.Store.ETag,
                Address = model.Store.Address,
                CityName = model.Store.CityName,
                CountryName = model.Store.CountryName,
            };

            stor.RowKey = Guid.NewGuid().ToString();
            stor.PartitionKey = "store";


            await _noSqlStore.Add(stor);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductAndStoreViewModel model)
        {
            var produ = new Product
            {
                Color = model.Product.Color,
                ETag = model.Product.ETag,
                Name = model.Product.Name,
                PartitionKey = model.Product.PartitionKey,
                Price = model.Product.Price,
                RowKey = model.Product.RowKey,
                Stock = model.Product.Stock,
                Timestamp = model.Product.Timestamp,
            };

            produ.RowKey = Guid.NewGuid().ToString();
            produ.PartitionKey = "Shoes";
            produ.StoreKey = model.Product.StoreKey;

            await _noSqlStorage.Add(produ);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Update(string rowKey, string partitionKey)
        {
            var product = await _noSqlStorage.Get(rowKey, partitionKey);
            ViewBag.products = (await _noSqlStorage.All()).ToList();
            ViewBag.stores = (await _noSqlStore.All()).ToList();
            ViewBag.IsUpdate = true;

            var model = new ProductAndStoreViewModel { Product = product };

            return View("Index", model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Product product)
        {
            ViewBag.IsUpdate = true;
            product.ETag = "*";
            try
            {
                await _noSqlStorage.Update(product);
                return RedirectToAction("Index");

            }
            catch (StorageException ex)
            {
                ex.RequestInformation.HttpStatusCode = (int)HttpStatusCode.PreconditionFailed;
                throw;
            }

        }

        [HttpGet]
        public async Task<IActionResult> Delete(string rowKey, string partitionKey)
        {

            await _noSqlStorage.Delete(rowKey, partitionKey);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Query(int filterPrice)
        {
            ViewBag.IsUpdate = false;
            ViewBag.products = (await _noSqlStorage.Query(x => x.Price > filterPrice)).ToList();
            return View("Index");
        }
    }
}
