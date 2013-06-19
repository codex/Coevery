﻿using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Coevery.Projections.Services;
using Coevery.Projections.ViewModels;
using Orchard;
using Orchard.Core.Contents.Controllers;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Themes;

namespace Coevery.Projections.Controllers
{
    public class SystemAdminController : Controller
    {
        private readonly IProjectionService _projectionService;

        public SystemAdminController(
             IOrchardServices services,
            IShapeFactory shapeFactory,
            IProjectionService projectionService)
        {
            _projectionService = projectionService;
            Services = services;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult List(string id)
        {
            return View();
        }

        public ActionResult Create(string id)
        {
            var pluralService = PluralizationService.CreateService(new CultureInfo("en-US"));
            if (pluralService.IsPlural(id))
            {
                id = pluralService.Singularize(id);
            }

           var viewModel = _projectionService.CreateTempProjection(id);
           return RedirectToAction("Edit", new { subId = viewModel.Id });
        }

        public ActionResult Edit(int id)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to edit queries")))
                return new HttpUnauthorizedResult();
            ProjectionEditViewModel viewModel = _projectionService.GetProjectionViewModel(id);
           
            return View(viewModel);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditPOST(int subId, ProjectionEditViewModel viewModel, string picklist, string returnUrl) {
            var pickArray = picklist.Split(new char[] {'$'}).Where(c=>!string.IsNullOrEmpty(c)).ToList();
            pickArray = pickArray.Select(c => c + ":Value").ToList();
            bool suc = _projectionService.EditPost(subId, viewModel, pickArray);
            return new EmptyResult();
        }
    }
}