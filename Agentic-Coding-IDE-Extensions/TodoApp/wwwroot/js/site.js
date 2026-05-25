// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', function () {
    const checkSvg = '<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-check-lg" viewBox="0 0 16 16" aria-hidden="true"><path d="M13.485 1.929a.75.75 0 0 1 1.06 1.06L6.53 10.006a.75.75 0 0 1-1.06 0L1.455 6.0a.75.75 0 1 1 1.06-1.06L6 8.425l7.485-6.496z"/></svg>';
    const undoSvg = '<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-arrow-counterclockwise" viewBox="0 0 16 16" aria-hidden="true"><path fill-rule="evenodd" d="M8 3a5 5 0 1 0 4.546 2.914.5.5 0 0 1 .908-.417A6 6 0 1 1 8 2v1z"/><path d="M8 1v4l3-2-3-2z"/></svg>';

    function getAntiForgeryToken() {
        const tokenInput = document.querySelector('#anti-forgery-form input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : null;
    }

    function renderTodoCard(todo) {
        const isDone = todo.isDone;
        const due = todo.dueDate ? new Date(todo.dueDate).toLocaleDateString(undefined, { month: 'short', day: '2-digit', year: 'numeric' }) : '';
        const created = new Date(todo.createdAt).toLocaleDateString(undefined, { month: 'short', day: '2-digit', year: 'numeric' });
        const projectName = todo.project?.name || '';

        return `
<div class="col-md-6 mb-3">
    <div id="todo-card-${todo.id}" class="card ${isDone ? 'border-success' : ''}" data-isdone="${isDone}">
        <div class="card-body">
            <h5 id="todo-title-${todo.id}" class="card-title ${isDone ? 'text-muted text-decoration-line-through' : ''}">${escapeHtml(todo.title || '')}</h5>
            ${todo.description ? `<p class="card-text text-muted">${escapeHtml(todo.description)}</p>` : ''}
            <div class="small text-secondary mb-3">
                <div>Created: ${created}</div>
                ${due ? `<div>Due: ${due}</div>` : ''}
                ${projectName ? `<div>Project: <span class="badge bg-info text-dark">${escapeHtml(projectName)}</span></div>` : ''}
                <div>Status: <span id="todo-status-${todo.id}" class="badge ${isDone ? 'bg-success' : 'bg-warning'}">${isDone ? 'Completed' : 'Pending'}</span></div>
            </div>
            <div class="btn-group" role="group">
                <form action="/Todos/ToggleComplete" method="post" class="d-inline toggle-complete-form" data-id="${todo.id}">
                        <input name="__RequestVerificationToken" type="hidden" value="${getAntiForgeryToken() || ''}" />
                        <input type="hidden" name="id" value="${todo.id}" />
                        <button type="submit" class="btn btn-sm ${isDone ? 'btn-outline-secondary' : 'btn-outline-success'}" aria-label="${isDone ? 'Undo' : 'Mark Completed'}" title="${isDone ? 'Undo' : 'Mark Completed'}">
                                ${isDone ? undoSvg : checkSvg}
                        </button>
                </form>
                <a href="/Todos/Edit/${todo.id}" class="btn btn-sm btn-outline-primary">Edit</a>
                <button type="button" data-id="${todo.id}" class="btn btn-sm btn-outline-danger btn-delete-ajax">Delete</button>
            </div>
        </div>
    </div>
</div>`;
    }

    function escapeHtml(unsafe) {
        return unsafe
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    function showToast(message, type = 'success', timeout = 5000) {
        const container = document.getElementById('toasts-container');
        if (!container) return;

        const toastId = `toast-${Date.now()}`;
        const bg = type === 'error' ? 'bg-danger text-white' : 'bg-success text-white';
        const toastHtml = `
<div id="${toastId}" class="toast ${bg}" role="alert" aria-live="assertive" aria-atomic="true" data-bs-delay="${timeout}">
    <div class="d-flex">
        <div class="toast-body">${escapeHtml(message)}</div>
        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
    </div>
</div>`;

        container.insertAdjacentHTML('beforeend', toastHtml);
        const el = document.getElementById(toastId);
        const bsToast = new bootstrap.Toast(el, { delay: timeout, autohide: true });
        bsToast.show();
        el.addEventListener('hidden.bs.toast', () => el.remove());
    }

    async function bindToggleForms(root = document) {
        root.querySelectorAll('.toggle-complete-form').forEach(form => {
            if (form.dataset.bound) return;
            form.dataset.bound = '1';
            form.addEventListener('submit', async (e) => {
                e.preventDefault();
                const id = form.dataset.id;
                const btn = form.querySelector('button[type=submit]');
                const formData = new FormData(form);

                try {
                    const response = await fetch(form.action, {
                        method: 'POST',
                        headers: { 'X-Requested-With': 'XMLHttpRequest' },
                        body: formData
                    });

                    if (!response.ok) throw new Error('Network response was not ok');
                    const data = await response.json();
                    if (!data || !data.success) throw new Error('Unexpected response');

                    const isDone = data.isDone;
                    const card = document.getElementById(`todo-card-${id}`);
                    const title = document.getElementById(`todo-title-${id}`);
                    const status = document.getElementById(`todo-status-${id}`);

                    if (isDone) {
                        card.classList.add('border-success');
                        if (title) title.classList.add('text-muted', 'text-decoration-line-through');
                        if (status) { status.classList.remove('bg-warning'); status.classList.add('bg-success'); status.textContent = 'Completed'; }
                        if (btn) { btn.classList.remove('btn-outline-success'); btn.classList.add('btn-outline-secondary'); btn.innerHTML = undoSvg; btn.setAttribute('aria-label','Undo'); btn.title = 'Undo'; }
                    } else {
                        card.classList.remove('border-success');
                        if (title) title.classList.remove('text-muted', 'text-decoration-line-through');
                        if (status) { status.classList.remove('bg-success'); status.classList.add('bg-warning'); status.textContent = 'Pending'; }
                        if (btn) { btn.classList.remove('btn-outline-secondary'); btn.classList.add('btn-outline-success'); btn.innerHTML = checkSvg; btn.setAttribute('aria-label','Mark Completed'); btn.title = 'Mark Completed'; }
                    }
                    showToast(isDone ? 'Todo marked completed.' : 'Todo marked pending.');
                }
                catch (err) {
                    console.error(err);
                    window.location.reload();
                }
            });
        });
    }

    function bindDeleteButtons(root = document) {
        root.querySelectorAll('.btn-delete-ajax').forEach(btn => {
            if (btn.dataset.bound) return;
            btn.dataset.bound = '1';
            btn.addEventListener('click', async () => {
                const id = btn.dataset.id;
                if (!confirm('Delete this todo?')) return;
                try {
                    const formData = new FormData();
                    formData.append('id', id);
                    const token = getAntiForgeryToken();
                    if (token) formData.append('__RequestVerificationToken', token);

                    const response = await fetch(`/Todos/Delete/${id}`, {
                        method: 'POST',
                        headers: { 'X-Requested-With': 'XMLHttpRequest' },
                        body: formData
                    });

                    if (!response.ok) throw new Error('Network response was not ok');
                    const data = await response.json();
                    if (!data || !data.success) throw new Error('Unexpected response');

                    const card = document.getElementById(`todo-card-${id}`);
                    if (card) {
                        const col = card.closest('.col-md-6');
                        if (col) col.remove();
                    }

                    const list = document.getElementById('todos-list');
                    if (!list || list.children.length === 0) {
                        const empty = document.getElementById('empty-message');
                        if (empty) empty.style.display = 'block';
                    }
                    showToast('Todo deleted.');
                }
                catch (err) {
                    console.error(err);
                    window.location.reload();
                }
            });
        });
    }

    function bindAjaxForms(root = document) {
        root.querySelectorAll('form.ajax-form').forEach(form => {
            if (form.dataset.bound) return;
            form.dataset.bound = '1';
            form.addEventListener('submit', async (e) => {
                e.preventDefault();
                const formData = new FormData(form);
                const token = getAntiForgeryToken();
                if (token && !formData.has('__RequestVerificationToken')) formData.append('__RequestVerificationToken', token);

                try {
                    const response = await fetch(form.action, {
                        method: 'POST',
                        headers: { 'X-Requested-With': 'XMLHttpRequest' },
                        body: formData
                    });

                    if (!response.ok) throw new Error('Network response was not ok');
                    const data = await response.json();
                    if (!data || !data.success) throw new Error('Unexpected response');

                    if (data.todo) {
                        const list = document.getElementById('todos-list');
                        const html = renderTodoCard(data.todo);
                        if (list) {
                            const existing = document.getElementById(`todo-card-${data.todo.id}`);
                            if (existing) {
                                const col = existing.closest('.col-md-6');
                                if (col) col.outerHTML = html;
                                showToast('Todo updated.');
                            } else {
                                list.insertAdjacentHTML('afterbegin', html);
                                const empty = document.getElementById('empty-message');
                                if (empty) empty.style.display = 'none';
                                showToast('Todo created.');
                            }

                            bindToggleForms(document);
                            bindDeleteButtons(document);
                        } else {
                            window.location.href = '/Todos';
                        }
                    } else {
                        window.location.href = '/Todos';
                    }
                }
                catch (err) {
                    console.error(err);
                    form.submit();
                }
            });
        });
    }

    bindToggleForms(document);
    bindDeleteButtons(document);
    bindAjaxForms(document);

    const toastsContainer = document.getElementById('toasts-container');
    if (toastsContainer) {
        const s = toastsContainer.dataset.tempSuccess;
        const e = toastsContainer.dataset.tempError;
        if (s) showToast(s, 'success');
        if (e) showToast(e, 'error');
    }
});

