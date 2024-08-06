const connection = new signalR.HubConnectionBuilder().withUrl("/chat").build();

const urlParams = new URLSearchParams(window.location.search);
const chatId = urlParams.get('id');

let chatsList = document.getElementById("chatsList")
let messagesList = document.getElementById("messagesList")
let chat = document.getElementById("chat")
let chatEntryTemplate = document.getElementById("chatEntry")
let messageTemplate = document.getElementById("messageTemplate")
let messagesInput = document.getElementById("messageInput")
let sendButton = document.getElementById("sendButton")

function setChat(chat) {
    let chatEntry = chatEntryTemplate.content.cloneNode(true)
    chatEntry.querySelector("label").textContent = chat
    chatEntry.querySelector("div").addEventListener("click", () => {
        window.location.href = "/Home/Chats?id=" + chat
    })
    chatsList.appendChild(chatEntry)
}

function setMessage(msg) {
    let message = messageTemplate.content.cloneNode(true)
    message.querySelector("label").textContent = msg.text
    messagesList.appendChild(message)
}

if (chatId) {
    chatsList.style.display = "none"

    connection.on("GetChatMessages", (msgs) => {
        for (let i = 0; i < msgs.length; i++) {
            setMessage(msgs[i])
        }
        messagesList.scrollTop = messagesList.scrollHeight;
    })

    connection.on("ReceiveMessage", (msg) => {
        setMessage(msg)
        messagesList.scrollTop = messagesList.scrollHeight;
    })

    connection.start().then(() => {
        connection.invoke("ConnectToChat", chatId).then(() => {
            connection.invoke("GetChatMessages", chatId)
        })
    })

    sendButton.addEventListener("click", () => {
        let text = messagesInput.value
        connection.invoke("SendMessage", text, chatId)
        messagesInput.value = ""
    })

} else {
    chat.style.display = "none"
    connection.on("GetChats", (chats) => {
        for (let i = 0; i < chats.length; i++) {
            setChat(chats[i])
        }
    })

    connection.start().then(() => {
        connection.invoke("GetChats")
    })
}
