export class NameEntryHandler{

    constructor(nameTemplate, contentList, selfAccountId) {
        this.nameTemplate = nameTemplate
        this.contentList = contentList
        this.lastAccountId = null
        this.selfAccountId = selfAccountId
    }

    setAccountsDict(accountsList) {
        this.accountsDict = {}
        for (let account of accountsList) {
            this.accountsDict[account.id] = {
                name: account.name,
                lastName: account.lastName
            }
        }
        console.log(this.accountsDict)
    }

    getNameString(account) {
        return account.name + " " + account.lastName
    }

    setName(accountId) {
        if (accountId == this.lastAccountId) {
            return
        }
        this.lastAccountId = accountId
        let account = this.accountsDict[accountId]
        let nameEntry = this.nameTemplate.content.cloneNode(true)
        let nameStr = this.getNameString(account)
        nameEntry.querySelector("label").textContent = nameStr
        if (accountId == this.selfAccountId) {
            nameEntry.querySelector("div").classList.add("rightSideMsg")
        } else {
            nameEntry.querySelector("div").classList.add("leftSideMsg")
        }
        this.contentList.appendChild(nameEntry)
    }

}