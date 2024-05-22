import type {UmbControllerHost} from '@umbraco-cms/backoffice/controller-api';
import {UmbWorkspaceActionBase} from '@umbraco-cms/backoffice/workspace';
import {UMB_DOCUMENT_WORKSPACE_CONTEXT} from "@umbraco-cms/backoffice/document";

export class UmbDocumentAiButtonWorkspaceAction extends UmbWorkspaceActionBase {

  _contentInfo: any;
   _showButton = false;
   _data: any;
  workspaceContext: any;
  constructor(host: UmbControllerHost, args: any) {
    super(host, args);
    this.disable();
    this.workspaceContext = this.getContext(UMB_DOCUMENT_WORKSPACE_CONTEXT)
        .then((context) => {
          this._data = context.getData();
          this.#isTranslatable().then(() => {
            if (this._showButton) {
              this.enable();
            }
          });
        });
  }

  #translate = async () => {
    var response = await fetch("/api/ai/content-translate/" + this._data.unique);
    this._contentInfo = await response.json();
  }

  #isTranslatable = async () => {
    const response = await fetch("/api/ai/content-is-translatable/" + this._data.unique);
    const data = await response.json();
    this._showButton = data.isTranslatable;
  }

  async execute() {
    await this.#translate();
  }
}

export { UmbDocumentAiButtonWorkspaceAction as api };