document.querySelectorAll('#editor-toolbar [data-cmd]').forEach(btn => {
    btn.addEventListener('click', () => {
        document.execCommand(btn.dataset.cmd, false, null);
        document.getElementById('recipe-steps').focus();
    });
});
 
document.getElementById('undo-btn').addEventListener('click', () => {
    document.execCommand('undo');
});
document.getElementById('redo-btn').addEventListener('click', () => {
    document.execCommand('redo');
});


const mediaUpload = document.getElementById('media-upload');
const mediaInput = document.getElementById('media-input');
const mediaPreview = document.getElementById('media-preview');
const mediaPlaceholder = document.getElementById('media-placeholder');
 
mediaUpload.addEventListener('click', () => mediaInput.click());
 
mediaUpload.addEventListener('dragover', (e) => {
    e.preventDefault();
    mediaUpload.style.borderColor = 'var(--accent)';
});
 
mediaUpload.addEventListener('dragleave', () => {
    mediaUpload.style.borderColor = '';
});
 
mediaUpload.addEventListener('drop', (e) => {
    e.preventDefault();
    mediaUpload.style.borderColor = '';
    const file = e.dataTransfer.files[0];
    if (file) handleMediaFile(file);
});
 
mediaInput.addEventListener('change', () => {
    if (mediaInput.files[0]) handleMediaFile(mediaInput.files[0]);
});
 
function handleMediaFile(file) {
    if (!file.type.startsWith('image/') && !file.type.startsWith('video/')) {
        alert('Оберіть зображення або відео');
        return;
    }
    const url = URL.createObjectURL(file);
    mediaPreview.src = url;
    mediaPreview.style.display = 'block';
    mediaPlaceholder.style.display = 'none';
}