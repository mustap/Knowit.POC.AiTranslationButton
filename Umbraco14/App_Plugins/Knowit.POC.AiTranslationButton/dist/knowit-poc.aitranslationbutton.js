var p = (t, a, s) => {
  if (!a.has(t))
    throw TypeError("Cannot " + s);
};
var h = (t, a, s) => (p(t, a, "read from private field"), s ? s.call(t) : a.get(t)), c = (t, a, s) => {
  if (a.has(t))
    throw TypeError("Cannot add the same private member more than once");
  a instanceof WeakSet ? a.add(t) : a.set(t, s);
}, r = (t, a, s, i) => (p(t, a, "write to private field"), i ? i.call(t, s) : a.set(t, s), s);
import { UmbWorkspaceActionBase as u } from "@umbraco-cms/backoffice/workspace";
import { UMB_DOCUMENT_WORKSPACE_CONTEXT as _ } from "@umbraco-cms/backoffice/document";
var e, n;
class m extends u {
  constructor(s, i) {
    super(s, i);
    c(this, e, void 0);
    c(this, n, void 0);
    this._showButton = !1, r(this, e, async () => {
      var o = await fetch("/api/ai/content-translate/" + this._data.unique);
      this._contentInfo = await o.json();
    }), r(this, n, async () => {
      const l = await (await fetch("/api/ai/content-is-translatable/" + this._data.unique)).json();
      this._showButton = l.isTranslatable;
    }), this.disable(), this.workspaceContext = this.getContext(_).then((o) => {
      this._data = o.getData(), h(this, n).call(this).then(() => {
        this._showButton && this.enable();
      });
    });
  }
  async execute() {
    await h(this, e).call(this);
  }
}
e = new WeakMap(), n = new WeakMap();
export {
  m as UmbDocumentAiButtonWorkspaceAction,
  m as api
};
//# sourceMappingURL=knowit-poc.aitranslationbutton.js.map
