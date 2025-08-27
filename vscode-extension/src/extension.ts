import * as vscode from 'vscode';

export function activate(context: vscode.ExtensionContext) {
    console.log('ECEngine Language Support is now active!');

    // Register a command to show information about ECEngine
    let disposable = vscode.commands.registerCommand('ecengine.showInfo', () => {
        vscode.window.showInformationMessage('ECEngine Language Support is active! Syntax highlighting enabled for .ec files.');
    });

    context.subscriptions.push(disposable);

    // Register a hover provider for ECEngine files
    const hoverProvider = vscode.languages.registerHoverProvider('ecengine', {
        provideHover(document, position, token) {
            const range = document.getWordRangeAtPosition(position);
            const word = document.getText(range);

            // Provide hover information for ECEngine keywords
            const keywordInfo: { [key: string]: string } = {
                'var': 'Declares a variable with function scope',
                'let': 'Declares a block-scoped local variable',
                'const': 'Declares a block-scoped read-only named constant',
                'function': 'Declares a function',
                'return': 'Exits a function and returns a value',
                'console': 'Provides access to debugging console methods'
            };

            if (keywordInfo[word]) {
                return new vscode.Hover(new vscode.MarkdownString(`**${word}**: ${keywordInfo[word]}`));
            }
        }
    });

    context.subscriptions.push(hoverProvider);

    // Register a completion provider
    const completionProvider = vscode.languages.registerCompletionItemProvider('ecengine', {
        provideCompletionItems(document: vscode.TextDocument, position: vscode.Position) {
            const completions: vscode.CompletionItem[] = [];

            // Add keyword completions
            const keywords = ['var', 'let', 'const', 'function', 'return', 'if', 'else', 'while', 'for'];
            keywords.forEach(keyword => {
                const item = new vscode.CompletionItem(keyword, vscode.CompletionItemKind.Keyword);
                item.detail = `ECEngine keyword: ${keyword}`;
                completions.push(item);
            });

            // Add console methods
            const consoleMethods = ['log', 'error', 'warn', 'info'];
            consoleMethods.forEach(method => {
                const item = new vscode.CompletionItem(`console.${method}`, vscode.CompletionItemKind.Method);
                item.detail = `Console method: ${method}`;
                item.insertText = new vscode.SnippetString(`console.${method}(\${1:message});`);
                completions.push(item);
            });

            return completions;
        }
    }, '.');

    context.subscriptions.push(completionProvider);
}

export function deactivate() {
    console.log('ECEngine Language Support is now deactivated.');
}
