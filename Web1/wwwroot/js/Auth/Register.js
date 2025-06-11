var form = document.getElementById("registerForm");
/*form.addEventListener("submit", function (event) {
    event.preventDefault();
    if (checkForm() === false) {
//            alert("Please check the form for errors");
            event.stopPropagation();
    } else {
//            alert("Form is correct, submitting");
            form.submit();
    }
    form.classList.add("was-validated");
}, false);

function checkForm() {
    var correct = true;
    var inputs = document.querySelectorAll("input");
    inputs.forEach(function (input) {
        if (!checkField(input)) {
            correct = false;
        }
    });
    return correct;
}
function checkField(Field) {
    if (Field.id === "" || Field.id === null) return true;
    var ErrorSpam = document.getElementById(Field.id + "Error");
    console.log(ErrorSpam)
    return true;
    //console.log("Checking field: " + Field.id + " with value: " + Field.value + " and error: " + ErrorSpam);
    if (Field.value.trim()) {
        ErrorSpam.style.display = 'none';
        correct = false;
        return true;
    } else {
        console.log("error in " + Field.id)
        ErrorSpam.style.display = 'block';
        return false;
    }
};
*/
var nameInput = document.getElementById("registerName");
var emailInput = document.getElementById("registerEmail");
var passwordInput = document.getElementById("registerPassword");
var confirmPasswordInput = document.getElementById("registerPasswordConfirm");

nameInput.addEventListener("keydown", function (event) {
    if (event.key === "Enter") {
        event.preventDefault();
        emailInput.focus();
    };
});

emailInput.addEventListener("keydown", function (event) {
    if (event.key === "Enter") {
        event.preventDefault();
        passwordInput.focus();
    };
});

passwordInput.addEventListener("keydown", function (event) {
    if (event.key === "Enter") {
        event.preventDefault();
        confirmPasswordInput.focus();
    };
});

confirmPasswordInput.addEventListener("keydown", function (event) {
    if (event.key === "Enter") {
        event.preventDefault();
        console.log(form);
        form.querySelector(".form-submit").click();
    };
});
document.querySelectorAll('input').forEach(input => {
    input.addEventListener('input', function () {
        const errorSpan = document.getElementById(this.id + "Error");
        if (errorSpan) {
            errorSpan.style.display = this.value.trim() ? 'none' : 'block';
        }
    });
});
