// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Lets the bin name preset dropdown fill in (or hand off to) the real Name field.
document.addEventListener('DOMContentLoaded', function () {
    var preset = document.getElementById('namePreset');
    var nameInput = document.getElementById('Name');
    if (!preset || !nameInput) {
        return;
    }

    preset.addEventListener('change', function () {
        if (preset.value === '__custom') {
            nameInput.value = '';
            nameInput.focus();
        } else if (preset.value) {
            nameInput.value = preset.value;
        }
    });
});
