(function () {
  Object.defineProperty(navigator, "languages", {
    get: () => ["en-US", "en"],
  });

  Object.defineProperty(navigator, "plugins", {
    get: () => [
      ""
    ],
  });

  Object.defineProperty(navigator, "webdriver", {
    get: () => false,
  });

  const _getParameter = WebGLRenderingContext.getParameter;

  WebGLRenderingContext.prototype.getParameter = function (flag) {
    if (flag === 37445)
      return '';
    if (flag === 37446)
      return '';

    return _getParameter(flag);
    };

    SAVED1 = window.document.documentElement.getAttribute("selenium");
    window.document.documentElement.removeAttribute("selenium");
    SAVED2 = window.document.documentElement.getAttribute("driver");
    window.document.documentElement.removeAttribute("driver");
    SAVED3 = window.document.documentElement.getAttribute("webdriver");
    window.document.documentElement.removeAttribute("webdriver");
    SAVED4 = navigator.webdriver;
    navigator.webdriver = void 0;
    //window.XPathResult = void 0;
    document.XPathResult = void 0;

})();