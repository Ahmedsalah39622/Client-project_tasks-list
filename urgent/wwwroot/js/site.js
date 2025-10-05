// Function to show the collection input form
function showCollectionInput(taskId) {
    // Hide any visible note input forms
    document.querySelectorAll('[id^=note-input-]').forEach(el => el.style.display = 'none');
    
    // Hide any other visible collection input forms
    document.querySelectorAll('[id^=collection-input-]').forEach(el => {
        if (el.id !== `collection-input-${taskId}`) {
            el.style.display = 'none';
        }
    });

    // Show the collection input form for this task
    const collectionInput = document.getElementById(`collection-input-${taskId}`);
    if (collectionInput) {
        collectionInput.style.display = 'block';
        // Focus on the input field
        const inputField = document.getElementById(`CollectionAmount-${taskId}`);
        if (inputField) {
            inputField.focus();
        }
    }
}

// Function to show the note input form
function showNoteInput(taskId) {
    // Hide any visible collection input forms
    document.querySelectorAll('[id^=collection-input-]').forEach(el => el.style.display = 'none');
    
    // Hide any other visible note input forms
    document.querySelectorAll('[id^=note-input-]').forEach(el => {
        if (el.id !== `note-input-${taskId}`) {
            el.style.display = 'none';
        }
    });

    // Show the note input form for this task
    const noteInput = document.getElementById(`note-input-${taskId}`);
    if (noteInput) {
        noteInput.style.display = 'block';
        // Focus on the textarea
        const textarea = document.getElementById(`NoteContent-${taskId}`);
        if (textarea) {
            textarea.focus();
        }
    }
}
