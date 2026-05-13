const canvas = document.getElementById("matrixCanvas");
const ctx = canvas.getContext("2d");

canvas.height = window.innerHeight;
canvas.width = window.innerWidth;

const letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@#$%^&*()*&^%";
const lettersArray = letters.split("");

const fontSize = 14;
const columns = canvas.width / fontSize;

const drops = [];

for (let i = 0; i < columns; i++) {
    drops[i] = 1;
}

function draw() {
    ctx.fillStyle = "rgba(0, 0, 0, 0.05)";
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    ctx.fillStyle = "#0F0";
    ctx.font = fontSize + "px monospace";

    for (let i = 0; i < drops.length; i++) {
        const text = lettersArray[Math.floor(Math.random() * lettersArray.length)];
        ctx.fillText(text, i * fontSize, drops[i] * fontSize);

        if (drops[i] * fontSize > canvas.height && Math.random() > 0.975) {
            drops[i] = 0;
        }

        drops[i]++;
    }
}

setInterval(draw, 33);