function obtenerJsonDesdeElemento(idElemento) {
    const elemento = document.getElementById(idElemento);
    if (!elemento) {
        return [];
    }

    try {
        return JSON.parse(elemento.textContent ?? "[]");
    } catch {
        return [];
    }
}

function crearTarjetaTasa(tasa, estaActiva) {
    const claseActiva = estaActiva ? " tarjeta-tasa-activa" : "";
    const rangoHasta = tasa.montoHastaUsd === null ? "En adelante" : `Hasta ${Number(tasa.montoHastaUsd).toLocaleString("es-MX", { minimumFractionDigits: 2, maximumFractionDigits: 2 })} USD`;

    return `
        <article class="tarjeta-tasa${claseActiva}">
            <div class="flex items-start justify-between gap-3">
                <div>
                    <p class="text-xs font-black uppercase tracking-[0.18em] text-slate-500">${tasa.nombrePais}</p>
                    <h3 class="mt-2 text-lg font-black text-tinta">${tasa.nombreSucursal}</h3>
                </div>
                <span class="rounded-full bg-azulClaroMarca px-3 py-1 text-sm font-black text-azulMarca">
                    ${Number(tasa.tasaCambio).toFixed(2)}
                </span>
            </div>
            <p class="mt-3 text-4xl font-black leading-none text-tinta">${Number(tasa.tasaCambio).toFixed(2)}</p>
            <p class="mt-3 text-sm leading-6 text-slate-600">
                Desde ${Number(tasa.montoDesdeUsd).toLocaleString("es-MX", { minimumFractionDigits: 2, maximumFractionDigits: 2 })} USD
                <span class="block">${rangoHasta}</span>
            </p>
        </article>`;
}

function inicializarCalculadoraPublica() {
    const modulo = document.getElementById("modulo-calculadora-publica");
    if (!modulo) {
        return;
    }

    const sucursales = obtenerJsonDesdeElemento("datos-sucursales-calculadora");
    const tasas = obtenerJsonDesdeElemento("datos-tasas-calculadora");
    const formulario = document.getElementById("formulario-calculadora");
    const selectorPais = document.getElementById("paisId");
    const selectorSucursal = document.getElementById("sucursalId");
    const campoMonto = document.getElementById("Solicitud_MontoUsd");
    const contenedorResultado = document.getElementById("contenedor-resultado");
    const contenedorMenores = document.getElementById("tasas-menores");
    const contenedorMayores = document.getElementById("tasas-mayores");
    const descripcionTasas = document.getElementById("descripcion-tasas");
    const botonEjemplo = document.getElementById("boton-ejemplo");
    const urlCalcular = modulo.dataset.urlCalcular;
    const paisInicial = modulo.dataset.paisInicial ?? "";
    const sucursalInicial = modulo.dataset.sucursalInicial ?? "";
    let temporizadorCalculo = null;
    let debeDesplazarResultado = false;

    function desplazarResultadoEnMovil() {
        if (window.innerWidth >= 768) {
            return;
        }

        const margenSuperior = 88;
        const posicionObjetivo = contenedorResultado.getBoundingClientRect().top + window.scrollY - margenSuperior;

        window.scrollTo({
            top: Math.max(posicionObjetivo, 0),
            behavior: "smooth"
        });
    }

    function obtenerPaisSeleccionado() {
        return Number(selectorPais.value || 0);
    }

    function obtenerSucursalSeleccionada() {
        return Number(selectorSucursal.value || 0);
    }

    function obtenerMontoActual() {
        return Number(campoMonto.value || 0);
    }

    function obtenerTasasFiltradasPorPais() {
        const paisId = obtenerPaisSeleccionado();
        return tasas.filter(tasa => tasa.paisId === paisId);
    }

    function obtenerTasaAplicableEnPantalla() {
        const sucursalId = obtenerSucursalSeleccionada();
        const monto = obtenerMontoActual();
        if (!sucursalId || monto <= 0) {
            return null;
        }

        return obtenerTasasFiltradasPorPais().find(tasa =>
            tasa.sucursalId === sucursalId &&
            monto >= Number(tasa.montoDesdeUsd) &&
            (tasa.montoHastaUsd === null || monto <= Number(tasa.montoHastaUsd)));
    }

    function renderizarTasas() {
        const tasasPais = obtenerTasasFiltradasPorPais();
        const tasaAplicable = obtenerTasaAplicableEnPantalla();
        const menores = tasasPais.filter(tasa => Number(tasa.montoDesdeUsd) < 1000);
        const mayores = tasasPais.filter(tasa => Number(tasa.montoDesdeUsd) >= 1000 || tasa.montoHastaUsd === null);
        const nombrePais = selectorPais.options[selectorPais.selectedIndex]?.text || "el pais seleccionado";

        descripcionTasas.textContent = `Tasas vigentes para ${nombrePais}.`;

        contenedorMenores.innerHTML = menores.length
            ? menores.map(tasa => crearTarjetaTasa(tasa, tasaAplicable != null && tasa.sucursalId === tasaAplicable.sucursalId && tasa.montoDesdeUsd === tasaAplicable.montoDesdeUsd)).join("")
            : "<p class='rounded-3xl border border-borde bg-white p-4 text-sm text-slate-500'>No hay tasas configuradas para este rango.</p>";

        contenedorMayores.innerHTML = mayores.length
            ? mayores.map(tasa => crearTarjetaTasa(tasa, tasaAplicable != null && tasa.sucursalId === tasaAplicable.sucursalId && tasa.montoDesdeUsd === tasaAplicable.montoDesdeUsd)).join("")
            : "<p class='rounded-3xl border border-borde bg-white p-4 text-sm text-slate-500'>No hay tasas configuradas para este rango.</p>";
    }

    function cargarSucursales(esCargaInicial) {
        const paisId = obtenerPaisSeleccionado();
        selectorSucursal.innerHTML = "";

        const opcionInicial = document.createElement("option");
        opcionInicial.value = "";
        opcionInicial.textContent = paisId ? "Selecciona el canal" : "Selecciona primero un pais";
        selectorSucursal.appendChild(opcionInicial);

        if (!paisId) {
            renderizarTasas();
            return;
        }

        const sucursalesFiltradas = sucursales.filter(sucursal => sucursal.paisId === paisId);
        sucursalesFiltradas.forEach(sucursal => {
            const opcion = document.createElement("option");
            opcion.value = String(sucursal.id);
            opcion.textContent = sucursal.nombre;
            selectorSucursal.appendChild(opcion);
        });

        if (esCargaInicial && sucursalInicial) {
            selectorSucursal.value = sucursalInicial;
        } else if (sucursalesFiltradas.length > 0) {
            selectorSucursal.value = String(sucursalesFiltradas[0].id);
        }

        renderizarTasas();
    }

    async function calcularCotizacion() {
        const paisId = obtenerPaisSeleccionado();
        const sucursalId = obtenerSucursalSeleccionada();
        const monto = obtenerMontoActual();

        renderizarTasas();

        if (!paisId || !sucursalId || monto <= 0) {
            contenedorResultado.innerHTML = "<div class='rounded-[1.75rem] border border-borde bg-white p-5 text-sm leading-7 text-slate-500'>Selecciona pais, canal e ingresa un monto valido para ver tu cotizacion.</div>";
            return;
        }

        contenedorResultado.innerHTML = "<div class='rounded-[1.75rem] border border-azulMarca/20 bg-azulClaroMarca p-5 text-sm leading-7 text-azulMarca'>Actualizando cotizacion...</div>";

        const respuesta = await fetch(urlCalcular, {
            method: "POST",
            body: new FormData(formulario),
            headers: { "X-Requested-With": "XMLHttpRequest" }
        });

        contenedorResultado.innerHTML = await respuesta.text();
        if (debeDesplazarResultado) {
            desplazarResultadoEnMovil();
        }

        debeDesplazarResultado = false;
    }

    function programarCalculo(opciones = {}) {
        const { desplazar = false } = opciones;

        if (temporizadorCalculo !== null) {
            window.clearTimeout(temporizadorCalculo);
        }

        debeDesplazarResultado = desplazar;
        temporizadorCalculo = window.setTimeout(() => {
            calcularCotizacion();
        }, 280);
    }

    selectorPais.value = paisInicial;
    cargarSucursales(true);

    selectorPais.addEventListener("change", () => {
        cargarSucursales(false);
        programarCalculo();
    });

    selectorSucursal.addEventListener("change", programarCalculo);
    campoMonto.addEventListener("input", () => programarCalculo());
    campoMonto.addEventListener("blur", () => programarCalculo({ desplazar: true }));

    botonEjemplo?.addEventListener("click", () => {
        campoMonto.value = "1250";
        programarCalculo({ desplazar: true });
    });

    formulario.addEventListener("submit", async evento => {
        evento.preventDefault();
        await calcularCotizacion();
    });

    renderizarTasas();
    if (campoMonto.value) {
        programarCalculo();
    }
}

function inicializarFormularioTasaCambio() {
    const formulario = document.getElementById("formulario-tasa-cambio");
    if (!formulario) {
        return;
    }

    const selectorPais = document.getElementById("paisId");
    const selectorSucursal = document.getElementById("sucursalId");
    if (!selectorPais || !selectorSucursal) {
        return;
    }

    const sucursales = obtenerJsonDesdeElemento("datos-sucursales-tasa-cambio");
    const sucursalSeleccionada = Number(formulario.dataset.sucursalSeleccionada || 0);

    function cargarSucursalesPorPais() {
        const paisId = Number(selectorPais.value || 0);
        selectorSucursal.innerHTML = "";

        const opcionInicial = document.createElement("option");
        opcionInicial.value = "";
        opcionInicial.textContent = paisId ? "Selecciona una sucursal o canal" : "Selecciona primero un pais";
        selectorSucursal.appendChild(opcionInicial);

        if (!paisId) {
            return;
        }

        const sucursalesFiltradas = sucursales.filter(sucursal => Number(sucursal.paisId) === paisId);
        sucursalesFiltradas.forEach(sucursal => {
            const opcion = document.createElement("option");
            opcion.value = String(sucursal.id);
            opcion.textContent = sucursal.nombre;
            opcion.selected = sucursalSeleccionada > 0 && sucursalSeleccionada === Number(sucursal.id);
            selectorSucursal.appendChild(opcion);
        });
    }

    selectorPais.addEventListener("change", cargarSucursalesPorPais);
    cargarSucursalesPorPais();
}

function inicializarToasts() {
    const toasts = document.querySelectorAll("[data-toast]");
    if (toasts.length === 0) {
        return;
    }

    toasts.forEach(toast => {
        const botonCerrar = toast.querySelector("[data-toast-cerrar]");
        toast.classList.add("toast-oculto");

        window.requestAnimationFrame(() => {
            window.requestAnimationFrame(() => {
                toast.classList.remove("toast-oculto");
            });
        });

        function cerrarToast() {
            toast.classList.add("toast-oculto");
            window.setTimeout(() => {
                toast.remove();
            }, 220);
        }

        botonCerrar?.addEventListener("click", cerrarToast);
        window.setTimeout(cerrarToast, 4200);
    });
}

document.addEventListener("DOMContentLoaded", () => {
    inicializarCalculadoraPublica();
    inicializarFormularioTasaCambio();
    inicializarToasts();
});
