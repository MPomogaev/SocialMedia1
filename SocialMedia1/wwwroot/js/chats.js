const connection = new signalR.HubConnectionBuilder().withUrl("/chat").build();

const urlParams = new URLSearchParams(window.location.search);
const chatId = urlParams.get('id');
let lastYear = (new Date()).getFullYear()
let selfId
let lastMsgDate

let searchBar = document.getElementById("searchBar")
let chatsList = document.getElementById("chatsList")
let messagesList = document.getElementById("messagesList")
let friendsList = document.getElementById("friendsList")
let chat = document.getElementById("chat")
let chatEntryTemplate = document.getElementById("chatEntry")
let messageTemplate = document.getElementById("messageTemplate")
let dateEntryTemplate = document.getElementById("dateEntry")
let friendEntryTemplate = document.getElementById("friendEntryTemplate")
let messagesInput = document.getElementById("messageInput")
let sendButton = document.getElementById("sendButton")
let nameTextInput = document.getElementById("nameTextInput")


function setFriendToFriendsList(friend) {
    let friendEntry = friendEntryTemplate.content.cloneNode(true)
    friendEntry.querySelector("label").textContent = friend.name
    friendEntry.querySelector(".friendId").value = friend.id
    const imgElement = friendEntry.querySelector("img");
    imgElement.src = "data:image/png;base64," + friend.profilePhoto;
    imgElement.addEventListener("click", () => {
        let url = "/Home/Account?id=" + friend.id
        window.location.href = url
    })
    friendsList.appendChild(friendEntry)
}

function goToCreateChat() {
    document.getElementById("createChatLabel").style.display = "none"
    document.getElementById("chatsList").style.display = "none"
    document.getElementById("createChatBlock").style.display = "flex"
    connection.on("GetFriends", (friends) => {
        for (const friend of friends) {
            setFriendToFriendsList(friend)
        }
    })
    connection.invoke("GetFriends")
}

function createChat() {
    let name = nameTextInput.value
    console.log(name)
    if (name == "") {
        return
    }
    let chatMembersIds = []
    friendsList.querySelectorAll(".friendEntryTemplate").forEach((child) => {
        let chosen = child.querySelector(".friendChosenCheckbox").checked
        if (chosen) {
            let friendId = child.querySelector(".friendId").value
            chatMembersIds.push(parseInt(friendId))
        }
    })
    console.log(chatMembersIds)
    connection.on("ChatCreated", (chatId) => {
        let url = "/Home/Chats?id=" + chatId
        window.location.href = url
    })
    connection.invoke("CreateChat", name, chatMembersIds)
}

function showChatContent() {
    document.querySelector(".pageContent").style.display = "block"
}

function setChat(chat) {
    let chatEntry = chatEntryTemplate.content.cloneNode(true)
    chatEntry.querySelector("label").textContent = chat.name
    chatEntry.querySelector("div").addEventListener("click", () => {
        window.location.href = "/Home/Chats?id=" + chat.id
    })
    chatsList.appendChild(chatEntry)
}

function compareDates(firstDate, secondDate) {
    return firstDate.getDate() == secondDate.getDate()
        && firstDate.getFullYear() == secondDate.getFullYear()
}

function appendDateToMsgList(msgDate) {
    if (lastMsgDate == null || !compareDates(lastMsgDate, msgDate)) {
        lastMsgDate = msgDate
        let dateEntry = dateEntryTemplate.content.cloneNode(true)
        let dateStr = lastMsgDate.getDate() + " "
            + lastMsgDate.toLocaleString('default', { month: 'long' })
        let year = msgDate.getFullYear()
        if (lastYear != year) {
            lastYear = year
            dateStr += " " + year
        }
        dateEntry.querySelector("label").textContent = dateStr
        messagesList.appendChild(dateEntry)
    } 
}

function setMessage(msg) {
    let message = messageTemplate.content.cloneNode(true)
    message.querySelector("label").textContent = msg.text
    let date = new Date(msg.createdDate)
    message.querySelector("messageDate").textContent =
        date.getHours() + ":"
        + date.getMinutes() + ":"
        + date.getSeconds()
    if (msg.accountId == selfId) {
        message.querySelector("div").classList.add("rightSideMsg")
    } else {
        message.querySelector("div").classList.add("leftSideMsg")
    }
    appendDateToMsgList(date)
    messagesList.appendChild(message)
}

if (chatId) {
    chatsList.style.display = "none"
    showChatContent()
    lastMsgDate = null;

    connection.on("GetChatMessages", (msgs) => {
        messagesList.innerHTML = ""
        for (let i = 0; i < msgs.length; i++) {
            setMessage(msgs[i])
        }
        messagesList.scrollTop = messagesList.scrollHeight;
    })

    connection.on("ReceiveMessage", (msg) => {
        setMessage(msg)
        messagesList.scrollTop = messagesList.scrollHeight;
    })

    connection.on("GetSelfAccountId", (accId) => {
        selfId = accId
    })

    connection.start().then(() => {
        connection.invoke("GetSelfAccountId")
        connection.invoke("ConnectToChat", chatId).then(() => {
            connection.invoke("GetChatMessages", chatId, searchBar.value)
        })
    })

    sendButton.addEventListener("click", () => {
        let text = messagesInput.value
        connection.invoke("SendMessage", text, chatId)
        messagesInput.value = ""
    })

    searchBar.addEventListener("input", () => {
        connection.invoke("GetChatMessages", chatId, searchBar.value)
    })

} else {
    chat.style.display = "none"
    showChatContent()

    connection.on("GetChats", (chats) => {
        for (let i = 0; i < chats.length; i++) {
            setChat(chats[i])
        }
    })

    connection.start().then(() => {
        connection.invoke("GetChats")
    })
}
