using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnityOfWork _unityOfWork;
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public int OrderTotal { get; set; }
        public CartController(IUnityOfWork unityOfWork)
        {
            _unityOfWork = unityOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unityOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == claim.Value,
                includeProperties: "Product")
            };

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, 
                    cart.Product.Price, cart.Product.Price50, cart.Product.Price100);

                ShoppingCartVM.CartTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            //var claimsIdentity = (ClaimsIdentity)User.Identity;
            //var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //ShoppingCartVM = new ShoppingCartVM()
            //{
            //    ListCart = _unityOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == claim.Value,
            //    includeProperties: "Product")
            //};

            //foreach (var cart in ShoppingCartVM.ListCart)
            //{
            //    cart.Price = GetPriceBasedOnQuantity(cart.Count,
            //        cart.Product.Price, cart.Product.Price50, cart.Product.Price100);

            //    ShoppingCartVM.CartTotal += (cart.Price * cart.Count);
            //}

            return View();
        }

        public IActionResult Plus(int cartId)
        {
            var cart = _unityOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
            _unityOfWork.ShoppingCart.IncrementCount(cart, 1);
            _unityOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _unityOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);

            if (cart.Count <= 1)
            {
                _unityOfWork.ShoppingCart.Remove(cart);
            }
            else
            {
                _unityOfWork.ShoppingCart.DecrementCount(cart, 1);
            }

            _unityOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cart = _unityOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
            _unityOfWork.ShoppingCart.Remove(cart);
            _unityOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        private double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
        {
            if (quantity <= 50)
            {
                return price;
            }
            else
            {
                if (quantity <= 100)
                {
                    return price50;
                }

                return price100;
            }
        }
    }
}
