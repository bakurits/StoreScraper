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

})();