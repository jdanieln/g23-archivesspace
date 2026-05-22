document.addEventListener('DOMContentLoaded', () => {
    const treeContainer = document.getElementById('hierarchy-tree-container');
    if (!treeContainer) return;

    let selectedNode = null;

    // --- Drag and Drop Logic ---
    const nodes = document.querySelectorAll('.hierarchy-node');
    nodes.forEach(node => {
        node.addEventListener('dragstart', handleDragStart);
        node.addEventListener('dragover', handleDragOver);
        node.addEventListener('drop', handleDrop);
        node.addEventListener('dragend', handleDragEnd);
        node.addEventListener('click', (e) => selectNode(node));
    });

    let dragSourceEl = null;

    function handleDragStart(e) {
        dragSourceEl = this;
        this.classList.add('dragging');
        e.dataTransfer.effectAllowed = 'move';
        e.dataTransfer.setData('text/html', this.innerHTML);
    }

    function handleDragOver(e) {
        if (e.preventDefault) {
            e.preventDefault();
        }
        e.dataTransfer.dropEffect = 'move';
        return false;
    }

    function handleDrop(e) {
        if (e.stopPropagation) {
            e.stopPropagation();
        }

        if (dragSourceEl !== this) {
            // Swap node indentation levels and parent metadata if dragging is drop-sorted
            const sourceIndex = Array.from(treeContainer.children).indexOf(dragSourceEl);
            const targetIndex = Array.from(treeContainer.children).indexOf(this);

            if (sourceIndex < targetIndex) {
                this.after(dragSourceEl);
            } else {
                this.before(dragSourceEl);
            }

            saveHierarchyState();
        }
        return false;
    }

    function handleDragEnd(e) {
        this.classList.remove('dragging');
        nodes.forEach(n => n.classList.remove('dragging'));
    }

    // --- Keyboard Navigation (US 33 & 34) ---
    document.addEventListener('keydown', (e) => {
        if (!selectedNode) return;

        const siblings = Array.from(treeContainer.children);
        const index = siblings.indexOf(selectedNode);

        if (e.key === 'ArrowDown') {
            e.preventDefault();
            if (index < siblings.length - 1) {
                selectNode(siblings[index + 1]);
            }
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            if (index > 0) {
                selectNode(siblings[index - 1]);
            }
        } else if (e.key === 'ArrowRight') {
            // Indent / Shift parent level
            e.preventDefault();
            let indent = getIndentLevel(selectedNode);
            if (indent < 4) {
                setIndentLevel(selectedNode, indent + 1);
                saveHierarchyState();
            }
        } else if (e.key === 'ArrowLeft') {
            // Outdent
            e.preventDefault();
            let indent = getIndentLevel(selectedNode);
            if (indent > 0) {
                setIndentLevel(selectedNode, indent - 1);
                saveHierarchyState();
            }
        } else if (e.key === 'ArrowUp' && e.altKey) {
            // Move up in position order
            e.preventDefault();
            if (index > 0) {
                siblings[index - 1].before(selectedNode);
                saveHierarchyState();
            }
        } else if (e.key === 'ArrowDown' && e.altKey) {
            // Move down in position order
            e.preventDefault();
            if (index < siblings.length - 1) {
                siblings[index + 1].after(selectedNode);
                saveHierarchyState();
            }
        }
    });

    function selectNode(node) {
        if (selectedNode) {
            selectedNode.classList.remove('keyboard-selected');
        }
        selectedNode = node;
        selectedNode.classList.add('keyboard-selected');
        selectedNode.focus();
    }

    function getIndentLevel(node) {
        for (let i = 0; i <= 4; i++) {
            if (node.classList.contains(`hierarchy-indent-${i}`)) {
                return i;
            }
        }
        return 0;
    }

    function setIndentLevel(node, level) {
        for (let i = 0; i <= 4; i++) {
            node.classList.remove(`hierarchy-indent-${i}`);
        }
        node.classList.add(`hierarchy-indent-${level}`);
        node.dataset.level = level;
    }

    // --- Save State to C# EF Core Backend ---
    function saveHierarchyState() {
        const siblings = Array.from(treeContainer.children);
        const updates = [];

        // Build list with estimated parent IDs based on indentation levels
        let parentTrack = { 0: null, 1: null, 2: null, 3: null, 4: null };

        siblings.forEach((node, pos) => {
            const id = parseInt(node.dataset.id);
            const level = getIndentLevel(node);

            // Record parent at previous indentation level
            let parentId = level === 0 ? 0 : parentTrack[level - 1];

            updates.push({
                id: id,
                parentId: parentId,
                position: pos
            });

            // Update parent track cache
            parentTrack[level] = id;
        });

        // Trigger POST back
        fetch('/Resource/UpdateHierarchy', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(updates)
        })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                showToast("Cambios guardados con éxito.");
            } else {
                showToast("Error al guardar: " + data.error, true);
            }
        })
        .catch(err => {
            showToast("Error en la conexión con el servidor.", true);
        });
    }

    // Simple toast helper
    function showToast(message, isError = false) {
        const toast = document.createElement('div');
        toast.style.position = 'fixed';
        toast.style.bottom = '20px';
        toast.style.right = '20px';
        toast.style.backgroundColor = isError ? '#ef4444' : '#10b981';
        toast.style.color = '#fff';
        toast.style.padding = '10px 20px';
        toast.style.borderRadius = '6px';
        toast.style.boxShadow = '0 4px 6px rgba(0,0,0,0.2)';
        toast.style.zIndex = '1000';
        toast.style.fontFamily = 'sans-serif';
        toast.style.fontSize = '0.9rem';
        toast.style.fontWeight = '600';
        toast.innerText = message;

        document.body.appendChild(toast);
        setTimeout(() => toast.remove(), 3000);
    }
});
