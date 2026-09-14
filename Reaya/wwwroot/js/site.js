// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("click", (event) => {
	const selectTrigger = event.target.closest("[data-custom-select-trigger]");
	if (selectTrigger) {
		const wrapper = selectTrigger.closest("[data-select-picker]");
		const isOpen = wrapper?.classList.toggle("is-open");
		selectTrigger.setAttribute("aria-expanded", String(Boolean(isOpen)));
		return;
	}

	const selectOption = event.target.closest("[data-custom-select-option]");
	if (selectOption) {
		const wrapper = selectOption.closest("[data-select-picker]");
		const select = wrapper?.querySelector("select");
		if (select) {
			select.value = selectOption.dataset.customSelectOption;
			select.dispatchEvent(new Event("change", { bubbles: true }));
			wrapper.classList.remove("is-open");
			wrapper.querySelector("[data-custom-select-trigger]").setAttribute("aria-expanded", "false");
			updateCustomSelect(wrapper, select);
		}
		return;
	}

	const toggle = event.target.closest("[data-password-toggle]");
	if (!toggle) {
		return;
	}

	const input = document.getElementById(toggle.dataset.passwordToggle);
	if (!input) {
		return;
	}

	const isVisible = input.type === "text";
	input.type = isVisible ? "password" : "text";
	toggle.setAttribute("aria-pressed", String(!isVisible));
	toggle.setAttribute("aria-label", isVisible ? "Show password" : "Hide password");
});

function updateCustomSelect(wrapper, select) {
	const triggerText = wrapper.querySelector("[data-custom-select-text]");
	const selectedOption = select.options[select.selectedIndex];
	if (triggerText && selectedOption) {
		triggerText.textContent = selectedOption.textContent;
		triggerText.classList.toggle("is-placeholder", !select.value);
	}

	wrapper.querySelectorAll("[data-custom-select-option]").forEach((option) => {
		option.classList.toggle("is-selected", option.dataset.customSelectOption === select.value);
	});
}

document.querySelectorAll("[data-select-picker]").forEach((wrapper, index) => {
	const select = wrapper.querySelector("select");
	if (!select) {
		return;
	}

	const listId = `${select.id}-options-${index}`;
	const trigger = document.createElement("button");
	trigger.type = "button";
	trigger.className = "custom-select-trigger";
	trigger.setAttribute("data-custom-select-trigger", "");
	trigger.setAttribute("aria-haspopup", "listbox");
	trigger.setAttribute("aria-expanded", "false");
	trigger.setAttribute("aria-controls", listId);
	trigger.innerHTML = '<span data-custom-select-text></span><span class="custom-select-chevron" aria-hidden="true"></span>';

	const list = document.createElement("div");
	list.id = listId;
	list.className = "custom-select-menu";
	list.setAttribute("role", "listbox");
	select.querySelectorAll("option").forEach((option) => {
		const item = document.createElement("button");
		item.type = "button";
		item.className = "custom-select-option";
		item.setAttribute("role", "option");
		item.dataset.customSelectOption = option.value;
		item.textContent = option.textContent;
		list.appendChild(item);
	});

	wrapper.classList.add("custom-select-ready");
	wrapper.append(trigger, list);
	select.addEventListener("change", () => updateCustomSelect(wrapper, select));
	updateCustomSelect(wrapper, select);
});

document.addEventListener("click", (event) => {
	document.querySelectorAll("[data-select-picker].is-open").forEach((wrapper) => {
		if (!wrapper.contains(event.target)) {
			wrapper.classList.remove("is-open");
			wrapper.querySelector("[data-custom-select-trigger]")?.setAttribute("aria-expanded", "false");
		}
	});
});
