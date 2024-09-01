import { getRightSidebarContentSwitcher, ContentSwitcher } from './contentSwitcher.js'
import { DateEntryHandler, dateToString } from './dateHandler.js'
import { NameEntryHandler } from './nameHandler.js'

const connection = new signalR.HubConnectionBuilder().withUrl("/chat").build();

const urlParams = new URLSearchParams(window.location.search);
const chatId = urlParams.get('id');

let selfId

let searchBar = document.getElementById("searchBar")
let chatsList = document.getElementById("chatsList")
let messagesList = document.getElementById("messagesList")
let friendsList = document.getElementById("friendsList")
let chatEntryTemplate = document.getElementById("chatEntry")
let messageTemplate = document.getElementById("messageTemplate")
let dateEntryTemplate = document.getElementById("dateEntry")
let nameEntryTemplate = document.getElementById("nameEntry")
let friendEntryTemplate = document.getElementById("friendEntryTemplate")
let messagesInput = document.getElementById("messageInput")
let sendButton = document.getElementById("sendButton")
let nameTextInput = document.getElementById("nameTextInput")

const dateEntryHandler = new DateEntryHandler(dateEntryTemplate, messagesList)
let nameEntryHandler = new NameEntryHandler(nameEntryTemplate, messagesList)

const rightSideBarSwitcher = getRightSidebarContentSwitcher()
const pageContent = document.querySelector(".pageContent")
const mainContentSwitcher = new ContentSwitcher(pageContent.children, pageContent, "block")

const ChatTypes = {
    private: 1,
    public: 2
}

function isWhitespace(str) {
    return /^\s*$/.test(str);
}

function addFriendToFriendsList(friend, isChosen) {
    let friendEntry = friendEntryTemplate.content.cloneNode(true)
    friendEntry.querySelector("label").textContent = friend.name
    friendEntry.querySelector(".friendChosenCheckbox").checked = isChosen
    friendEntry.querySelector(".friendId").value = friend.id
    const imgElement = friendEntry.querySelector("img");
    imgElement.src = "data:image/png;base64," + friend.profilePhoto;
    imgElement.addEventListener("click", () => {
        let url = "/Home/Account?id=" + friend.id
        window.location.href = url
    })
    friendsList.appendChild(friendEntry)
}

function setChatOptionsContent() {
    rightSideBarSwitcher.setDefaults([])
    mainContentSwitcher.setDefaults(["createChatBlock"])
}

function setChatSaveButtonOnclick(action) {
    document.getElementById("chatSettingsButton")
        .addEventListener("click", action)
}

function startFriendsReceiving(action) {
    connection.on("GetFriends", (friends) => {
        for (const friend of friends) {
            action(friend)
        }
    })
    connection.invoke("GetFriends")
}

function getChatSettings() {
    let name = nameTextInput.value
    if (isWhitespace(name)) {
        return null
    }
    let chatMembersIds = []
    friendsList.querySelectorAll(".friendEntryTemplate").forEach((child) => {
        let chosen = child.querySelector(".friendChosenCheckbox").checked
        if (chosen) {
            let friendId = child.querySelector(".friendId").value
            chatMembersIds.push(parseInt(friendId))
        }
    })
    console.log(name, chatMembersIds)
    return { name: name, members: chatMembersIds }
}

function createChat() {
    const settings = getChatSettings()
    connection.on("ChatCreated", (chatId) => {
        let url = "/Home/Chats?id=" + chatId
        window.location.href = url
    })
    connection.invoke("CreateChat", settings.name, settings.members)
}

function editChat() {
    const settings = getChatSettings()
    connection.on("ChatEdited", (chatId) => {
        let url = "/Home/Chats?id=" + chatId
        window.location.href = url
    })
    connection.invoke("EditChat", chatId, settings.name, settings.members)
}

function leaveChat() {
    connection.on("LeftChat", () => {
        window.location.href = "/Home/Chats"
    })
    connection.invoke("LeaveChat", chatId);
}
window.leaveChat = leaveChat

function goToCreateChat() {
    setChatOptionsContent()
    setChatSaveButtonOnclick(createChat)
    startFriendsReceiving((friend) => {
        addFriendToFriendsList(friend, false)
    })
}
window.goToCreateChat = goToCreateChat

function goToEditChat() {
    setChatOptionsContent()
    setChatSaveButtonOnclick(editChat)
    connection.on("SetChatInfo", (name, members) => {
        console.log(name, members)
        nameTextInput.value = name
        startFriendsReceiving((friend) => {
            if (members.includes(friend.id)) {
                addFriendToFriendsList(friend, true)
            } else {
                addFriendToFriendsList(friend, false)
            }
        })
    })
    connection.invoke("GetChatInfo", parseInt(chatId))
}
window.goToEditChat = goToEditChat

function setChat(chat) {
    let chatEntry = chatEntryTemplate.content.cloneNode(true)
    let chatEntryLabel = chatEntry.querySelector("label")
    if (chat.chatTypeId != ChatTypes.private) {
        chatEntryLabel.style.fontWeight = 'bold'
    }
    chatEntryLabel.textContent = chat.name
    chatEntry.querySelector("div").addEventListener("click", () => {
        window.location.href = "/Home/Chats?id=" + chat.id
    })
    chatsList.appendChild(chatEntry)
}

function setMessage(msg) {
    let message = messageTemplate.content.cloneNode(true)
    message.querySelector("label").textContent = msg.text
    let date = new Date(msg.createdDate)
    message.querySelector("messageDate").textContent = dateToString(date)
    if (msg.accountId == selfId) {
        message.querySelector("div").classList.add("rightSideMsg")
    } else {
        message.querySelector("div").classList.add("leftSideMsg")
    }
    dateEntryHandler.setDate(date)
    nameEntryHandler.setName(msg.accountId)
    messagesList.appendChild(message)
}

if (chatId) {
    mainContentSwitcher.setDefaults(["chat"])

    connection.on("ReceiveChatTypeAndName", (type, name) => {
        document.getElementById("chatName").textContent = name
        if (type == ChatTypes.private) {
            rightSideBarSwitcher.setDefaults([])
        } else {
            rightSideBarSwitcher.setDefaults(["chatOptions"])
        }
    })

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
        nameEntryHandler = new NameEntryHandler(nameEntryTemplate, messagesList, selfId)
        connection.invoke("GetMembersAccounts", chatId)
    })

    connection.on("GetMembersAccounts", (membersAccounts) => {
        nameEntryHandler.setAccountsDict(membersAccounts)
        connection.invoke("GetChatMessages", chatId, searchBar.value)
    })

    connection.start().then(() => {
        connection.invoke("GetChatTypeAndName", chatId)
        connection.invoke("GetSelfAccountId")
        connection.invoke("ConnectToChat", chatId)
    })

    sendButton.addEventListener("click", () => {
        let text = messagesInput.value
        if (text == "") {
            return
        }
        connection.invoke("SendMessage", text, chatId)
        messagesInput.value = ""
    })

    searchBar.addEventListener("input", () => {
        connection.invoke("GetChatMessages", chatId, searchBar.value)
    })

} else {
    mainContentSwitcher.setDefaults(["chatsList"])
    rightSideBarSwitcher.setDefaults(["createChatLabel"])

    connection.on("GetChats", (chats) => {
        console.log(chats)
        for (let i = 0; i < chats.length; i++) {
            setChat(chats[i])
        }
    })

    connection.start().then(() => {
        connection.invoke("GetChats")
    })
}
