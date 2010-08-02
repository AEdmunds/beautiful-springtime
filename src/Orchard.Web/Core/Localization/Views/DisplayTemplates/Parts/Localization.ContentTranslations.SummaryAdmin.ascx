﻿<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.Localization.ViewModels.ContentLocalizationsViewModel>" %>
<%
    Html.RegisterStyle("admin.css"); %>
<div class="content-localization"><%
    if (Model.Localizations.Count() > 0) { %>
    <%--//todo: need this info in the view model--%>
    <div class="content-localizations"><h4><%:T("Translations:") %></h4><%:Html.UnorderedList(Model.Localizations, (c, i) => Html.ItemEditLink(c.Culture.Culture, c), "localizations") %></div><%
    } %>
    <div class="add-localization"><%:Html.ActionLink(T("+ New translation").Text, "translate", "admin", new { area = "Localization", id = Model.MasterId }, null)%></div>
</div>