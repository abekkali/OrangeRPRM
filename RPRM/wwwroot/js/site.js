// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Coordonnées pour Al-Quds
const AlQuds = {
    latitude: 31.7683,
    longitude: 35.2137,
    altitude: 0,
    altitudeReference: -1,
    name: "Al-Quds, Palestine"
};

// Fonction pour vérifier et mettre à jour la clé PSE
function FreePalestine() {

   localStorage.setItem('PSE', JSON.stringify(AlQuds));
}

function fetchLookupData() {
    return new Promise(function (resolve, reject) {
        var lookupData = localStorage.getItem('lookupData');
        var lastModified = new Date(localStorage.getItem('lastModified') || '').toUTCString();

        $.ajax({
            url: '/api/getlookupdata',
            headers: lastModified ? { 'If-Modified-Since': lastModified } : {},
            success: function (data, status, xhr) {
                var serverLastModified = xhr.getResponseHeader('Last-Modified');
                if (serverLastModified) {
                    localStorage.setItem('lastModified', serverLastModified);
                }

                if (status === 'notmodified') {
                    resolve(JSON.parse(lookupData));
                } else {
                    localStorage.setItem('lookupData', JSON.stringify(data));
                    resolve(data);
                }
            },
            error: handleError(reject)
        });
    });
}

function fetchGroupData() {
    return new Promise(function (resolve, reject) {
        var groupData = localStorage.getItem('groupData');
        var lastModifiedGroupe = localStorage.getItem('lastModifiedGroupe') || '';

        $.ajax({
            url: '/api/getgroupdata',
            headers: lastModifiedGroupe ? { 'If-Modified-Since': lastModifiedGroupe } : {},
            success: function (data, status, xhr) {
                var serverLastModifiedGroupe = xhr.getResponseHeader('Last-Modified');
                if (serverLastModifiedGroupe) {
                    localStorage.setItem('lastModifiedGroupe', serverLastModifiedGroupe);
                }

                if (status === 'notmodified') {
                    resolve(JSON.parse(groupData));
                } else {
                    localStorage.setItem('groupData', JSON.stringify(data));
                    resolve(data);
                }
            },
            error: handleError(reject)
        });
    });
}

function handleSuccess(resolve, dataKey, lastModifiedKey) {
    return function (data, status, xhr) {
        if (xhr.status === 200) {
            console.log('Server data is more recent, updating client data...');

            var lastModifiedServer = new Date(xhr.getResponseHeader(lastModifiedKey));
            localStorage.setItem(dataKey, JSON.stringify(data));
            localStorage.setItem(lastModifiedKey, lastModifiedServer.toUTCString());

            resolve(data); // Resolve the promise with the data
        } else if (xhr.status === 304) {
            console.log('Client data is up to date.');

            resolve(JSON.parse(localStorage.getItem(dataKey))); // Resolve the promise with the data from local storage
        }
    };
}

function handleError(reject) {
    return function (jqXHR, textStatus, errorThrown) {
        console.log('Error: ' + textStatus + ' ' + errorThrown);
        reject(new Error(errorThrown)); // Reject the promise with an error
    }
}
function fetchAndStoreData(url, dataKey, lastModifiedKey) {
    return new Promise(function (resolve, reject) {
        $.ajax({
            url: url,
            success: function (data, status, xhr) {
                var lastModifiedServer = new Date(xhr.getResponseHeader(lastModifiedKey));
                localStorage.setItem(dataKey, JSON.stringify(data));
                localStorage.setItem(lastModifiedKey, lastModifiedServer.toISOString());

                resolve(data);
            },
            error: handleError(reject)
        });
    });
}

document.addEventListener('DOMContentLoaded', function () {
    var selectAllCheckbox = document.getElementById('selectAll');
    if (selectAllCheckbox) {
    selectAllCheckbox.addEventListener('change', function () {
        var checkboxes = document.querySelectorAll('.form-check-input');
        for (var i = 0; i < checkboxes.length; i++) {
            if (checkboxes[i] !== selectAllCheckbox) {
                checkboxes[i].checked = selectAllCheckbox.checked;
            }
        }
    });
    }
});
function populateLookupDropdown(lookupData, type, selectId) {
    let select = $('#' + selectId);
    select.empty();
    select.append($('<option></option>').attr('value', '').text('Sélectionnez un choix'));
    let filteredData = lookupData.filter(item => item.lookup_Type === type);
    filteredData.forEach(function (item) {
        select.append($('<option></option>').attr('value', item.id).text(item.value));
    });
}
function selectDefaultOption(selectElement, defaultOptionValue) {
    $(selectElement).find('option').each(function () {
        if ($(this).text() === defaultOptionValue) {
            $(this).prop('selected', true);
            return false; // Break out of the loop once the option is selected
        }
    });
}
document.addEventListener('DOMContentLoaded', function () {
    var modifyButtons = document.querySelectorAll('[data-bs-target="#modify"]');

    modifyButtons.forEach(function (button) {
        button.addEventListener('click', async function () {
            var userId = button.getAttribute('data-id');

            var response = await fetch(`/Users/GetUserById?id=${userId}`);
            var userData = await response.json();

            document.getElementById('muserId').value = userId;
            document.getElementById('mfullName').value = userData.fullName;
            document.getElementById('mcompany').value = userData.company;
            document.getElementById('mPhone').value = userData.phoneNumber;

            var roleSelect = document.querySelector('.form-select');
            var options = roleSelect.options;
            for (var i = 0; i < options.length; i++) {
                if (options[i].value === userData.role) {
                    roleSelect.selectedIndex = i;
                    options[i].selected = true;
                } else {
                    options[i].selected = false;
                }
            }
        });
    });
});
//var delModal = document.getElementById('del');
//delModal.addEventListener('show.bs.modal', function (event) {
//    var button = event.relatedTarget;
//    var userId = button.getAttribute('data-id');
//    var userEmail = button.getAttribute('data-email');

//    var userIdElement = document.getElementById('userId');
//    var userEmailElement = document.getElementById('userEmail');
//    var selectedIdsElement = document.getElementById('selectedIds');

//    userIdElement.textContent = userId;
//    userEmailElement.textContent = userEmail;
//    selectedIdsElement.value = userId;
//});


document.addEventListener('DOMContentLoaded', function () {

    var delBtn = document.getElementById("delete-btn-check");
    // Vérifiez si delBtn existe
    if (delBtn) {
        delBtn.addEventListener('click', function () {
            var selectedCheckboxes = document.querySelectorAll('input[type="checkbox"][name="selectedIds"]:checked');
            var selectedIds = Array.from(selectedCheckboxes).map(function (checkbox) {
                return checkbox.value;
            }).join(',');
        });
    }
});


$(document).ready(function () {
    const requiredFields = document.querySelectorAll('input[required], select[required]');
    requiredFields.forEach(function (field) {
        const fieldId = field.id;
        const label = document.querySelector(`label[for="${fieldId}"]`);
        if (label) {
            label.classList.add('required-label');
        }
    });
    $('.copy-btn').on('click', function () {
        var valueToCopy = $(this).data('value');
        var $temp = $("<input>");
        $("body").append($temp);
        $temp.val(valueToCopy).select();
        document.execCommand("copy");
        $temp.remove();

        // Vous pouvez ajouter un message de confirmation ici, par exemple :
        //alert("Valeur copiée dans le presse-papiers");
    });
});
$(document).ready(function () {
    $(document).on('click', '.edit-btn', function () {
        // Récupérer les données stockées dans les attributs data-*
        var codePays = $(this).data("code-pays");
        var nomPays = $(this).data("nom-pays");
        var nomAng = $(this).data("nom-ang");
        var pass = $(this).data("pass");
        var continent = $(this).data("continent");
        var region = $(this).data("region");
        var mcc = $(this).data("mcc");
        var cc = $(this).data("cc");
        console.log(nomAng);
        // Mettez à jour les valeurs des champs cachés et input avec les données récupérées disabled
        $("#mCode_Pays_Hidden").val(codePays);
        $("#mNom_Pays_Hidden").val(nomPays);
        $("#mCode_Pays").val(codePays);
        $("#mNom_Pays").val(nomPays);
        $("#mNomAng").val(nomAng);
        $("#m_cc").val(cc);
        $("#mMCC").val(mcc);
        $("#mRegion").val(region);
        $("#mContinent").val(continent);

        // Sélectionnez l'option par défaut pour Pass en fonction de la valeur passs
        $("#mPass").val(pass === "oui" ? "oui" : "non");
    });
});
function convertToDateInputValue(dateString) {
    if (dateString !== null && dateString !== undefined) {
        const parts = dateString.split('/');
        const day = parts.length > 0 ? (parts[0].length === 1 ? '0' + parts[0] : parts[0]) : '';
        const month = parts.length > 1 ? (parts[1].length === 1 ? '0' + parts[1] : parts[1]) : '';
        const year = parts.length > 2 ? parts[2] : '';
        return `${year}-${month}-${day}`;
    }

    return '';
}



function updateCheckboxes(permissions) {
    // Parcourez les permissions et mettez à jour les cases à cocher en conséquence
    permissions.forEach(function (permission) {

        var className = permission.className;
        var canView = permission.canView;
        var canEdit = permission.canEdit;
        console.log(className);
        console.log(canView);
        console.log(canEdit);
        // Sélectionnez les cases à cocher en fonction du nom de la classe
        var viewCheckbox = document.querySelector('input[data-classname="' + className + '"][data-permissiontype="view"]');
        var editCheckbox = document.querySelector('input[data-classname="' + className + '"][data-permissiontype="edit"]');

        // Mettez à jour l'état des cases à cocher en fonction des permissions
        if (viewCheckbox) {
            viewCheckbox.checked = canView;
        }
        if (editCheckbox) {
            editCheckbox.checked = canEdit;
        }
    });
} function resetCheckboxes() {
    var viewCheckboxes = document.querySelectorAll('input[data-permissiontype="view"]');
    var editCheckboxes = document.querySelectorAll('input[data-permissiontype="edit"]');

    viewCheckboxes.forEach(function (checkbox) {
        checkbox.checked = false;
    });

    editCheckboxes.forEach(function (checkbox) {
        checkbox.checked = false;
    });
}

$(document).ready(function () {
    $('#droitUser').on('show.bs.modal', async function (event) { 
        var button = $(event.relatedTarget);
        var userId = button.data('id');
        var modal = $(this);
        modal.find('#userId').val(userId);
        resetCheckboxes();
        var response = await fetch(`/Users/GetUserPermissions?userId=${userId}`);
        var perm = await response.json();
        updateCheckboxes(perm);
    });
});

function fillSelect(selectElement, items) {
    items.forEach(function (item) {
        const option = document.createElement('option');
        option.value = item.id;
        option.textContent = item.value;
        selectElement.appendChild(option);
    });
}
function loadPLMNCodes() {
    $.get("/api/GetAllPLMNCodes", function (data) {
        $("#plmnOptions").empty();
        data.forEach(function (plmn) {
            var option = $("<option>").val(plmn.code_PLMN).text(plmn.nom_Op + " - " + plmn.nom_pays);
            $("#plmnOptions").append(option);
        });
    });
}
$('#plmnSelect').on('change', function () {
    var val = $(this).val();
    var match = $('#plmnOptions option').filter(function () {
        return this.value == val;
    }).length;

    if (match == 0) {
        $('#plmnError').show();
        $(this).val("");
    } else {
        $('#plmnError').hide();
    }
});
$('#addcurrency, #mcurrency').on('change', function () {
    var val = $(this).val();
    var match = $('#currencyList option').filter(function () {
        return this.value == val;
    }).length;

    if (match == 0) {
        $(this).next('.error-message').show();
        $(this).val("");
    } else {
        $(this).next('.error-message').hide();
    }
});

function formatRemainingTime(endTime, isAccessRestricted) {
    // Convertit endTime en objet Date.
    var endTime = new Date(endTime);

    // Calculer le temps restant en millisecondes.
    var remainingTime = endTime.getTime() - new Date().getTime();

    // Formate le temps restant en fonction de sa durée.
    if (!isAccessRestricted || remainingTime <= 0) {
        return "Non restreint";
    } else if (remainingTime >= 365 * 24 * 60 * 60 * 1000) {
        return Math.floor(remainingTime / (365 * 24 * 60 * 60 * 1000)) + " an(s) restant(s)";
    } else if (remainingTime >= 30 * 24 * 60 * 60 * 1000) {
        return Math.floor(remainingTime / (30 * 24 * 60 * 60 * 1000)) + " mois restant(s)";
    } else if (remainingTime >= 24 * 60 * 60 * 1000) {
        return Math.floor(remainingTime / (24 * 60 * 60 * 1000)) + " jour(s) restant(s)";
    } else if (remainingTime >= 60 * 60 * 1000) {
        return Math.floor(remainingTime / (60 * 60 * 1000)) + " heure(s) restante(s)";
    } else {
        return Math.floor(remainingTime / (60 * 1000)) + " minute(s) restante(s)";
    }
}
 /***************************************************************************
  * ----------------------------Supprimer------------------------------------
  ***************************************************************************/
$(document).ready(function () {
    $("#delete-btn-check").on("click", function (e) {
        e.preventDefault();

        // Get the specific form for this delete button
        var form = $(this).closest('form');

        // Get selected elements
        var selectedElements = [];
        form.find('input[name="selectedIds"]:checked').each(function () {
            selectedElements.push($(this).val());
        });

        // Generate the modal dynamically
        var modalHTML = `
            <div class="modal fade" id="confirmationModal" tabindex="-1" aria-labelledby="confirmationModalLabel" aria-hidden="true">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="confirmationModalLabel">Confirmation de suppression</h5>
                            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                <span aria-hidden="true">&times;</span>
                            </button>
                        </div>
                        <div class="modal-body">
                            Êtes-vous sûr de vouloir supprimer les éléments suivants ?
                            <ul id="elements-to-delete">
                                ${selectedElements.map(element => `<li>${element}</li>`).join('')}
                            </ul>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-dismiss="modal">Non</button>
                            <button type="button" id="confirm-delete" class="btn btn-danger">Oui, supprimer</button>
                        </div>
                    </div>
                </div>
            </div>
        `;
        $("body").append(modalHTML);

        // Show the modal
        $("#confirmationModal").modal('show');

        // Attach a one-time event handler for the confirm-delete button
        $("#confirm-delete").one("click", function () {
            // Proceed with form submission
            form.off("submit").submit();

            // Remove the modal after use
            $("#confirmationModal").modal('hide').remove();
        });
    });

});
$(document).on("click", ".close, .btn-secondary", function () {
    $("#confirmationModal").modal('hide').remove();
});












