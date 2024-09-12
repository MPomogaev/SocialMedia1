import { DateEntryHandler, dateToString } from './dateHandler.js'
import { getRightSidebarContentSwitcher } from './contentSwitcher.js'

const connection = new signalR.HubConnectionBuilder().withUrl("/account").build();

const urlParams = new URLSearchParams(window.location.search);
const accountId = urlParams.get('id');

const friendsList = document.getElementById("friendsList")
const friendsListItemTemplate = document.getElementById("friendsListItemTemplate")
const postTemplate = document.getElementById("postTemplate")
const postDateTemplate = document.getElementById("postDateTemplate")

const accountDataBlock = document.getElementById("accountDataBlock")
const friendsBlock = document.getElementById("friendsBlock")
const postsBlock = document.getElementById("postsBlock")
const createPostBlock = document.getElementById("createPostBlock")

const dateEntryHandler = new DateEntryHandler(postDateTemplate, postsBlock)

const rightSidebarContentSwitcher = getRightSidebarContentSwitcher()

function setFriend(friend) {
    let friendEntry = friendsListItemTemplate.content.cloneNode(true)
    friendEntry.querySelector("label").textContent = friend.name
    const imgElement = friendEntry.querySelector("img");
    imgElement.src = "data:image/png;base64," + friend.profilePhoto;
    imgElement.addEventListener("click", () => {
        let url = "/Home/Account?id=" + friend.id
        window.location.href = url
    })
    friendsList.appendChild(friendEntry)
}

function setPost(post) {
    let postEntry = postTemplate.content.cloneNode(true)
    postEntry.querySelector(".postTitle").textContent = post.title
    postEntry.querySelector(".postText").textContent = post.text
    let postDate = new Date(post.createdDate)
    postEntry.querySelector(".postTime").textContent = dateToString(postDate)
    dateEntryHandler.setDate(postDate)
    postsBlock.appendChild(postEntry)
    console.log(post.text)
}

connection.on("GetFriends", (friends) => {
    for (const friend of friends) {
        setFriend(friend)
    }
})

connection.on("GetPosts", (posts) => {
    for (const post of posts) {
        setPost(post)
    }
})

connection.on("Post", () => {
    let url = "/Home/Account"
    window.location.href = url
})

try {
    connection.start().then(() => {
        connection.invoke("GetFriends", accountId)
        connection.invoke("GetPosts", accountId)
    })
} catch (ex) {
    console.log("couldn't connect")
}

if (accountId != null) {
    rightSidebarContentSwitcher.setDefaults([])
}

document.getElementById("createPostButton").addEventListener("click", () => {
    accountDataBlock.style.display = "none"
    friendsBlock.style.display = "none"
    postsBlock.style.display = "none"
    createPostBlock.style.display = "flex"
})

document.getElementById("postButton").addEventListener("click", () => {
    let text = document.getElementById("createPostTextarea").value
    if (text == "") {
        return
    }
    let topic = document.getElementById("createPostTopic").value
    connection.invoke("Post", text, topic)
})

