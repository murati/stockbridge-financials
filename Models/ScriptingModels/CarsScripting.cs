﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace StockbridgeFinancials.Models.ScriptingModels
{
    internal class CarsScripting
    {
        public string Message { get; set; }
        public string Script { get; set; }
        public bool IsNavigation { get; set; }
        public bool IsResultsPage { get; set; }

        public static List<CarsScripting> InitializeCarsScripting()
        {
            return new List<CarsScripting> {
                new CarsScripting { Message= "Menu clicked...", Script= "document.getElementsByClassName('nav-user-menu-button')[0].click()" },
                new CarsScripting { Message= "Sign-in clicked...",Script = "document.getElementsByClassName('global-header-container')[0].children[0].shadowRoot.lastElementChild.lastElementChild.lastElementChild.children[0].click()" },
                new CarsScripting { Message= "Login Name entered...",Script = "document.querySelector('[id=auth-modal-email]').value = 'johngerson808@gmail.com'" },
                new CarsScripting { Message= "Password entered...",Script = "document.querySelector('[id=auth-modal-current-password]').value = 'test8008'" },
                new CarsScripting { Message= "Sign-in Button clicked...",Script = "document.querySelector('[type=sign_in]').form.getElementsByTagName('ep-button')[0].click()", IsNavigation = true },
                new CarsScripting { Message= "Used Cars are being selected...",Script = "document.getElementById('make-model-search-stocktype').value = 'used'" },
                new CarsScripting { Message= "Used Cars selected!",Script = "document.getElementById('make-model-search-stocktype').dispatchEvent(new Event('change'))" },
                new CarsScripting { Message= "Preferred Car Make is being selected..." ,Script = "document.getElementById('makes').value = 'tesla'"},
                new CarsScripting { Message= "Preferred Car Make selected!",Script = "document.getElementById('makes').dispatchEvent(new Event('change'))" },
                new CarsScripting { Message= "Preferred Model is being selected...",Script = "document.getElementById('models').value = 'tesla-model_s'" },
                new CarsScripting { Message= "Preferred Model selected!",Script = "document.getElementById('models').dispatchEvent(new Event('change'))" },
                new CarsScripting { Message= "Price Limit is set...",Script = "document.getElementById('make-model-max-price').value='100000'" },
                new CarsScripting { Message= "Distance is set...",Script = "document.getElementById('make-model-maximum-distance').value='all'" },
                new CarsScripting { Message= "Zip Code is set...",Script = "document.getElementById('make-model-zip').value='94596'" },
                new CarsScripting { Message= "Searching...",Script = "document.forms[0].getElementsByTagName('button')[0].click()", IsNavigation = true, IsResultsPage = true},
                new CarsScripting { Message= "Moving to 2nd Page",Script = "document.querySelector('[id=pagination-direct-link-2]').click()", IsNavigation = true, IsResultsPage = true},
                new CarsScripting { Message= "Home Delivery Checking...",Script = "document.getElementsByName('home_delivery')[1].checked=true"},
                new CarsScripting { Message= "Home Delivery Checked!",Script = "document.getElementsByName('home_delivery')[1].click()", IsNavigation = true, IsResultsPage = true},
                new CarsScripting { Message= "Model X Checking...",Script = "document.querySelectorAll('[value=tesla-model_x]')[1].checked=true"},
                new CarsScripting { Message= "Model X Checked!",Script = "document.querySelectorAll('[value=tesla-model_x]')[1].click()", IsNavigation = true, IsResultsPage = true},
                new CarsScripting { Message= "Moving to 2nd Page",Script = "document.querySelector('[id=pagination-direct-link-2]').click()", IsNavigation = true, IsResultsPage = true},


            };
        }
    }
}
