export class ContentSwitcher {
    constructor(elements, elementsTable = null, elementsTableDefault = null) {
        this.elementsTable = elementsTable
        if (elementsTableDefault == null) {
            this.elementsTableDefault = this.__getDefault(this.elementsTable)
        } else {
            this.elementsTableDefault = elementsTableDefault
        }
        this.elements = elements
        this.defaultDisplays = {}
        for (const element of this.elements) {
            let defaultStyle = this.__getDefault(element)
            this.defaultDisplays[element.id] = defaultStyle
        }
        //console.log(this.elements)
        //console.log(this.defaultDisplays)
        //console.log(this.elementsTable)
        //console.log(this.elementsTableDefault)
    }

    setDefaults(elementsIds) {
        this.hideContentTable()
        for (let element of this.elements) {
            let id = element.id
            if (elementsIds.includes(id)) {
                element.style.display = this.defaultDisplays[id]
            } else {
                element.style.display = "none"
            }
        }
        this.showContentTable()
    }

    hideContentTable() {
        if (this.elementsTable != null)
            this.elementsTable.style.display = "none"
    }

    showContentTable() {
        if (this.elementsTable != null)
            this.elementsTable.style.display = this.elementsTableDefault
    }

    __getDefault(element) {
        let defaultStyle = window.getComputedStyle(element).display
        if (defaultStyle == "none") {
            defaultStyle = "flex"
        }
        return defaultStyle
    }
}

export function getRightSidebarContentSwitcher() {
    let rightSidebar = document.getElementById("rightSidebar")
    let children = rightSidebar.children
    return new ContentSwitcher(children, rightSidebar)
}