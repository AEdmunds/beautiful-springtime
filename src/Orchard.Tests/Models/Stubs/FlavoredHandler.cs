﻿using Orchard.Models.Driver;
using Orchard.Models.ViewModels;

namespace Orchard.Tests.Models.Stubs {
    public class FlavoredHandler : ContentHandler {
        public FlavoredHandler() {
            OnGetDisplayViewModel<Flavored>((ctx, part) => ctx.AddDisplay(new TemplateViewModel(part)));
        }
        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "beta" || context.ContentType == "alpha") {
                context.Builder.Weld<Flavored>();
            }
        }
    }
}