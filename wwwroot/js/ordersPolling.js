async function subscribe() {
    let root = document.querySelector("#currentOrders")
    let response = await fetch('/api/getOrdersPolling');
    if (response.status == 502) {
        await new Promise(resolve => setTimeout(resolve, 5000));
        await subscribe();
    }
    else if (response.status != 200) {
        console.log(response.statusText);
        await new Promise(resolve => setTimeout(resolve, 5000));
        await subscribe();
    }
    else {
        let message = await response.text();
        root.innerHTML = message;
        await new Promise(resolve => setTimeout(resolve, 5000));
        await subscribe();
    }
}

subscribe();
