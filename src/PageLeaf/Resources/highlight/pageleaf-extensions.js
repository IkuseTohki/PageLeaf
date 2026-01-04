function copyCode(button) {
    // ボタンの親（code-block-header）の隣にある pre/code を探す
    const container = button.closest('.code-block-container');
    if (!container) return;

    const codeElement = container.querySelector('pre code');
    if (!codeElement) return;

    // テキストを取得
    const text = codeElement.innerText;

    // クリップボードにコピー
    navigator.clipboard.writeText(text).then(() => {
        // フィードバック: クラスを付与してアイコンを切り替える
        button.classList.add('copied');

        setTimeout(() => {
            button.classList.remove('copied');
        }, 2000);
    }).catch(err => {
        console.error('Failed to copy: ', err);
        alert('Failed to copy to clipboard.');
    });
}
