using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace I2R.LightNews.Utilities;

public static class HtmlSanitiser
{
    private const string VoidElements = "area,br,col,hr,img,wbr";
    private const string OptionalEndTagBlockElements = "colgroup,dd,dt,li,p,tbody,td,tfoot,th,thead,tr";
    private const string OptionalEndTagInlineElements = "rp,rt";
    private const string OptionalEndTagElements = OptionalEndTagInlineElements + "," + OptionalEndTagBlockElements;
    private const string BlockElements = OptionalEndTagBlockElements + ",address,article,aside,blockquote,caption,center,del,dir,div,dl,figure,figcaption,footer,h1,h2,h3,h4,h5,h6,header,hgroup,hr,ins,map,menu,nav,ol,pre,section,table,ul";
    private const string InlineElements = OptionalEndTagInlineElements + ",a,abbr,acronym,b,bdi,bdo,big,cite,code,del,dfn,em,font,i,img,ins,kbd,label,map,mark,q,ruby,rp,rt,s,samp,small,span,strike,strong,sub,sup,time,tt,u,var";
    private const string DefaulValidElements = VoidElements + "," + BlockElements + "," + InlineElements + "," + OptionalEndTagElements;
    private const string DefaulUriAttrs = "background,cite,href,longdesc,src,xlink:href";
    private const string DefaulSrcsetAttrs = "srcset";
    private const string DefaultHtmlAttrs = "abbr,align,class,alt,axis,bgcolor,border,cellpadding,cellspacing,clear,color,cols,colspan,compact,coords,dir,face,headers,height,hreflang,hspace,ismap,lang,language,nohref,nowrap,rel,rev,rows,rowspan,rules,scope,scrolling,shape,size,span,start,summary,tabindex,target,title,type,valign,value,vspace,width";
    private const string DefaulValidAttrs = DefaulUriAttrs + "," + DefaulSrcsetAttrs + "," + DefaultHtmlAttrs;
    private static readonly ISet<string> ValidElements = DefaulValidElements.Split(',').ToHashSet(StringComparer.OrdinalIgnoreCase);
    private static readonly ISet<string> ValidAttributes = DefaulValidAttrs.Split(',').ToHashSet(StringComparer.OrdinalIgnoreCase);

    public static string SanitizeHtmlFragment(string html, string excludeSelectors = default) {
        var element = ParseHtmlFragment(html);
        for (var i = element.ChildNodes.Length - 1; i >= 0; i--) {
            Sanitize(element.ChildNodes[i], excludeSelectors);
        }


        return element.InnerHtml;
    }

    private static IElement ParseHtmlFragment(string content) {
        var uniqueId = Guid.NewGuid().ToString("D");
        var parser = new HtmlParser();
        var document = parser.ParseDocument($"<div id='{uniqueId}'>{content}</div>");
        var element = document.GetElementById(uniqueId);
        return element;
    }

    private static void Sanitize(INode node, string excludeSelectors = default) {
        if (node is IElement htmlElement) {
            if (excludeSelectors.HasValue()) {
                foreach (var selector in excludeSelectors.Split(',')) {
                    // Console.WriteLine(new {
                    //     selector,
                    //     tag = htmlElement.TagName,
                    //     classes = JsonSerializer.Serialize(htmlElement.ClassList.ToArray())
                    // });

                    if (selector.StartsWith(".")) {
                        if (htmlElement.ClassList.Contains(selector.Replace(".", ""))) {
                            Console.WriteLine("Removed: " + htmlElement.TagName + ", because of: " + selector);
                            htmlElement.Remove();
                            continue;
                        }
                    }

                    if (selector.StartsWith("#")) {
                        if (htmlElement.Id == selector.Replace("#", "")) {
                            Console.WriteLine("Removed: " + htmlElement.TagName + ", because of: " + selector);
                            htmlElement.Remove();
                            continue;
                        }
                    }

                    if (htmlElement.TagName == selector.ToUpper()) {
                        Console.WriteLine("Removed: " + htmlElement.TagName + ", because of: " + selector);
                        htmlElement.Remove();
                    }
                }
            }

            if (!ValidElements.Contains(htmlElement.TagName)) {
                htmlElement.Remove();
            }

            for (var i = htmlElement.Attributes.Length - 1; i >= 0; i--) {
                var attribute = htmlElement.Attributes[i];
                if (!ValidAttributes.Contains(attribute.Name)) {
                    htmlElement.RemoveAttribute(attribute.NamespaceUri, attribute.Name);
                }
            }
        }

        for (var i = node.ChildNodes.Length - 1; i >= 0; i--) {
            Sanitize(node.ChildNodes[i]);
        }
    }
}