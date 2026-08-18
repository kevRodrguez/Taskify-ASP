(function () {
    var board = document.querySelector(".kanban");
    if (!board) {
        return;
    }

    var projectId = board.getAttribute("data-project-id");
    var hubUrl = board.getAttribute("data-hub-url");
    var tokenInput = document.querySelector("#kanban-af input[name='__RequestVerificationToken']");
    var token = tokenInput ? tokenInput.value : "";
    var lastClientRequestId = null;
    var template = document.getElementById("task-card-template");

    function statusLists() {
        return Array.prototype.slice.call(board.querySelectorAll(".kanban-column__list"));
    }

    function listForStatus(status) {
        return board.querySelector('.kanban-column__list[data-status="' + status + '"]');
    }

    function cardById(taskItemId) {
        return board.querySelector('.kanban-card[data-task-id="' + taskItemId + '"]');
    }

    function formatDue(dueDate) {
        if (!dueDate) {
            return "";
        }
        var parts = String(dueDate).split("-");
        if (parts.length !== 3) {
            return dueDate;
        }
        return parts[2] + "/" + parts[1] + "/" + parts[0];
    }

    function applyCard(card, event) {
        card.setAttribute("data-task-id", event.taskItemId);
        var title = card.querySelector(".kanban-card__title");
        if (title) {
            title.textContent = event.title || "";
        }

        var assigned = card.querySelector(".assigned");
        if (assigned) {
            assigned.textContent = event.assignedToName || "";
            assigned.hidden = !event.assignedToName;
        }

        var due = card.querySelector(".due");
        if (due) {
            due.textContent = event.dueDate ? "Vence " + formatDue(event.dueDate) : "";
            due.hidden = !event.dueDate;
        }

        var edit = card.querySelector(".edit-link");
        if (edit) {
            edit.setAttribute("href", "/Tasks/Edit/" + event.taskItemId);
        }
    }

    function upsertCard(event) {
        var card = cardById(event.taskItemId);
        if (!card && template) {
            card = template.content.firstElementChild.cloneNode(true);
        }
        if (!card) {
            return;
        }

        applyCard(card, event);
        var list = listForStatus(event.status);
        if (!list) {
            return;
        }

        var children = list.querySelectorAll(".kanban-card");
        var index = Math.max(0, Math.min(event.sortOrder || 0, children.length));
        if (children[index]) {
            list.insertBefore(card, children[index]);
        } else {
            list.appendChild(card);
        }
    }

    function postStatus(taskItemId, status, sortOrder) {
        lastClientRequestId = crypto.randomUUID();
        return fetch("/Tasks/UpdateStatus?projectId=" + encodeURIComponent(projectId), {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                RequestVerificationToken: token
            },
            body: JSON.stringify({
                taskItemId: taskItemId,
                projectId: projectId,
                status: Number(status),
                sortOrder: sortOrder,
                clientRequestId: lastClientRequestId
            })
        });
    }

    statusLists().forEach(function (list) {
        Sortable.create(list, {
            group: "kanban",
            animation: 150,
            ghostClass: "kanban-card--ghost",
            onEnd: function (evt) {
                var card = evt.item;
                var taskItemId = card.getAttribute("data-task-id");
                var status = evt.to.getAttribute("data-status");
                var sortOrder = evt.newIndex;
                postStatus(taskItemId, status, sortOrder)
                    .then(function (res) {
                        if (!res.ok) {
                            throw new Error("update-failed");
                        }
                        return res.json();
                    })
                    .then(function (data) {
                        if (data && data.toast && window.showToast) {
                            window.showToast(data.toast.message, data.toast.type);
                        }
                    })
                    .catch(function () {
                        window.location.reload();
                    });
            }
        });
    });

    if (!window.signalR) {
        return;
    }

    var connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl)
        .withAutomaticReconnect()
        .build();

    connection.on("TaskUpdated", function (event) {
        if (!event) {
            return;
        }
        if (String(event.projectId).toLowerCase() !== String(projectId).toLowerCase()) {
            return;
        }
        if (event.clientRequestId && event.clientRequestId === lastClientRequestId) {
            return;
        }
        if (event.deleted) {
            var existing = cardById(event.taskItemId);
            if (existing) {
                existing.remove();
            }
            return;
        }
        upsertCard(event);
    });

    connection.start().then(function () {
        return connection.invoke("JoinProject", projectId);
    }).catch(function (err) {
        console.warn("No se pudo conectar al tablero en tiempo real.", err);
    });
})();
