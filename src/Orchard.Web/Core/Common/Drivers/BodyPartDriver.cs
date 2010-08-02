﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Services;
using Orchard.Core.Common.Settings;
using Orchard.Core.Common.ViewModels;
using Orchard.Core.ContentsLocation.Models;
using Orchard.Core.Routable.Models;
using Orchard.Services;

namespace Orchard.Core.Common.Drivers {
    [UsedImplicitly]
    public class BodyPartDriver : ContentPartDriver<BodyPart> {
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;

        private const string TemplateName = "Parts/Common.Body";
        private const string DefaultTextEditorTemplate = "TinyMceTextEditor";
        private const string PlainTextEditorTemplate = "PlainTextEditor";

        public BodyPartDriver(IOrchardServices services, IEnumerable<IHtmlFilter> htmlFilters) {
            _htmlFilters = htmlFilters;
            Services = services;
        }

        public IOrchardServices Services { get; set; }

        protected override string Prefix {
            get { return "Body"; }
        }

        // \/\/ Hackalicious on many accounts - don't copy what has been done here for the wrapper \/\/
        protected override DriverResult Display(BodyPart part, string displayType) {
            var bodyText = _htmlFilters.Aggregate(part.Text, (text, filter) => filter.ProcessContent(text));
            var model = new BodyDisplayViewModel { BodyPart = part, Text = bodyText };
            var location = part.GetLocation(displayType);

            return Combined(
                Services.Authorizer.Authorize(Permissions.ChangeOwner) ? ContentPartTemplate(model, "Parts/Common.Body.ManageWrapperPre").LongestMatch(displayType, "SummaryAdmin").Location(location) : null,
                Services.Authorizer.Authorize(Permissions.ChangeOwner) ? ContentPartTemplate(model, "Parts/Common.Body.Manage").LongestMatch(displayType, "SummaryAdmin").Location(location) : null,
                ContentPartTemplate(model, TemplateName, Prefix).LongestMatch(displayType, "Summary", "SummaryAdmin").Location(location),
                Services.Authorizer.Authorize(Permissions.ChangeOwner) ? ContentPartTemplate(model, "Parts/Common.Body.ManageWrapperPost").LongestMatch(displayType, "SummaryAdmin").Location(location) : null);
        }

        protected override DriverResult Editor(BodyPart part) {
            var model = BuildEditorViewModel(part);
            var location = part.GetLocation("Editor");
            return ContentPartTemplate(model, TemplateName, Prefix).Location(location);
        }

        protected override DriverResult Editor(BodyPart part, IUpdateModel updater) {
            var model = BuildEditorViewModel(part);
            updater.TryUpdateModel(model, Prefix, null, null);

            // only set the format if it has not yet been set to preserve the initial format type - might want to change this later to support changing body formats but...later
            if (string.IsNullOrWhiteSpace(model.Format))
                model.Format = GetFlavor(part);

            var location = part.GetLocation("Editor");
            return ContentPartTemplate(model, TemplateName, Prefix).Location(location);
        }

        private static BodyEditorViewModel BuildEditorViewModel(BodyPart part) {
            return new BodyEditorViewModel {
                BodyPart = part,
                TextEditorTemplate = GetFlavor(part) == "html" ? DefaultTextEditorTemplate : PlainTextEditorTemplate,
                AddMediaPath = new PathBuilder(part).AddContentType().AddContainerSlug().AddSlug().ToString()
            };
        }

        private static string GetFlavor(BodyPart part) {
            var typePartSettings = part.Settings.GetModel<BodyTypePartSettings>();
            return (typePartSettings != null && !string.IsNullOrWhiteSpace(typePartSettings.Flavor))
                       ? typePartSettings.Flavor
                       : part.PartDefinition.Settings.GetModel<BodyPartSettings>().FlavorDefault;
        }

        class PathBuilder {
            private readonly IContent _content;
            private string _path;

            public PathBuilder(IContent content) {
                _content = content;
                _path = "";
            }

            public override string ToString() {
                return _path;
            }

            public PathBuilder AddContentType() {
                Add(_content.ContentItem.ContentType);
                return this;
            }

            public PathBuilder AddContainerSlug() {
                var common = _content.As<ICommonPart>();
                if (common == null)
                    return this;

                var routable = common.Container.As<RoutePart>();
                if (routable == null)
                    return this;

                Add(routable.Slug);
                return this;
            }

            public PathBuilder AddSlug() {
                var routable = _content.As<RoutePart>();
                if (routable == null)
                    return this;

                Add(routable.Slug);
                return this;
            }

            private void Add(string segment) {
                if (string.IsNullOrEmpty(segment))
                    return;
                if (string.IsNullOrEmpty(_path))
                    _path = segment;
                else
                    _path = _path + "/" + segment;
            }
        }
    }
}