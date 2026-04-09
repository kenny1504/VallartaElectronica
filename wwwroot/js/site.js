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

function formatearMontoUsd(monto) {
    return Number(monto).toLocaleString("es-MX", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function convertirValorEntradaANumero(valor) {
    return Number(String(valor ?? "").replace(/,/g, "").trim());
}

function obtenerTextoRango(tasa) {
    const hasta = tasa.montoHastaUsd === null ? "En adelante" : formatearMontoUsd(tasa.montoHastaUsd);
    return `${formatearMontoUsd(tasa.montoDesdeUsd)} - ${hasta}`;
}

function crearFilaTasaCompacta(tasa, estaActiva) {
    const claseActiva = estaActiva ? " fila-tasa-activa" : "";

    return `
        <article class="fila-tasa-compacta${claseActiva}">
            <div class="min-w-0">
                <div class="flex flex-wrap items-center gap-2">
                    <p class="text-[11px] font-black uppercase tracking-[0.18em] text-slate-500">${tasa.nombrePais}</p>
                    <span class="rounded-full bg-slate-100 px-2.5 py-1 text-[10px] font-bold uppercase tracking-[0.16em] text-slate-600">${obtenerTextoRango(tasa)} USD</span>
                </div>
                <h3 class="mt-2 truncate text-base font-black text-tinta">${tasa.nombreSucursal}</h3>
            </div>
            <div class="text-right">
                <p class="text-2xl font-black leading-none text-tinta">${Number(tasa.tasaCambio).toFixed(2)}</p>
                <p class="mt-1 text-xs font-semibold text-slate-500">${tasa.codigoMoneda}</p>
            </div>
        </article>`;
}

function inicializarCalculadoraPublica() {
    const modulo = document.getElementById("modulo-calculadora-publica");
    if (!modulo) {
        return;
    }

    const sucursales = obtenerJsonDesdeElemento("datos-sucursales-calculadora");
    let tasas = obtenerJsonDesdeElemento("datos-tasas-calculadora");
    const formulario = document.getElementById("formulario-calculadora");
    const selectorPais = document.getElementById("paisId");
    const selectorSucursal = document.getElementById("sucursalId");
    const campoMonto = document.getElementById("Solicitud_MontoUsd");
    const campoMontoVisible = document.getElementById("montoVisible");
    const botonMonedaEntrada = document.getElementById("boton-moneda-entrada");
    const leyendaMonedaEntrada = document.getElementById("leyenda-moneda-entrada");
    const campoFechaCliente = document.getElementById("fechaCliente");
    const etiquetaFechaActualizacion = document.getElementById("fecha-actualizacion-cliente");
    const contenedorResultado = document.getElementById("contenedor-resultado");
    const contenedorListaTasas = document.getElementById("lista-tasas");
    const descripcionTasas = document.getElementById("descripcion-tasas");
    const tituloListadoTasas = document.getElementById("titulo-listado-tasas");
    const pestanaRangoMenor = document.getElementById("tab-rango-menor");
    const pestanaRangoMayor = document.getElementById("tab-rango-mayor");
    const botonEjemplo = document.getElementById("boton-ejemplo");
    const urlCalcular = modulo.dataset.urlCalcular;
    const urlTasas = modulo.dataset.urlTasas;
    const paisInicial = modulo.dataset.paisInicial ?? "";
    const sucursalInicial = modulo.dataset.sucursalInicial ?? "";
    let temporizadorCalculo = null;
    let debeDesplazarResultado = false;
    let rangoVisible = "menor";
    let ultimaClaveCalculada = "";
    let modoMonedaEntrada = "usd";

    function obtenerFechaLocalCliente() {
        const fecha = new Date();
        const anio = fecha.getFullYear();
        const mes = String(fecha.getMonth() + 1).padStart(2, "0");
        const dia = String(fecha.getDate()).padStart(2, "0");
        return `${anio}-${mes}-${dia}`;
    }

    function formatearFechaVisible(fechaIso) {
        const [anio, mes, dia] = fechaIso.split("-");
        return `${mes}/${dia}/${anio}`;
    }

    function establecerFechaCliente() {
        const fechaCliente = obtenerFechaLocalCliente();
        if (campoFechaCliente) {
            campoFechaCliente.value = fechaCliente;
        }

        if (etiquetaFechaActualizacion) {
            etiquetaFechaActualizacion.textContent = formatearFechaVisible(fechaCliente);
        }

        return fechaCliente;
    }

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

    function obtenerMontoVisibleActual() {
        return convertirValorEntradaANumero(campoMontoVisible?.value || 0);
    }

    function obtenerClaveSolicitudActual() {
        const paisId = obtenerPaisSeleccionado();
        const sucursalId = obtenerSucursalSeleccionada();
        const montoNormalizado = campoMonto.value.trim();
        const fechaCliente = campoFechaCliente?.value ?? "";
        return `${paisId}|${sucursalId}|${montoNormalizado}|${fechaCliente}`;
    }

    function obtenerTasasFiltradasPorPais() {
        const paisId = obtenerPaisSeleccionado();
        return tasas.filter(tasa => tasa.paisId === paisId);
    }

    function obtenerMonedaDestinoActual() {
        const resultadoActual = contenedorResultado.querySelector("[data-resultado-exitoso='true']");
        if (resultadoActual?.dataset.resultadoMoneda) {
            return resultadoActual.dataset.resultadoMoneda;
        }

        const primeraTasa = obtenerTasasFiltradasPorPais()[0];
        return primeraTasa?.codigoMoneda || "USD";
    }

    function obtenerRangoSugerido() {
        const monto = obtenerMontoActual();
        return monto >= 1000 ? "mayor" : "menor";
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

    function obtenerTasaReferenciaPorPagador() {
        const sucursalId = obtenerSucursalSeleccionada();
        if (!sucursalId) {
            return null;
        }

        const tasasDelPagador = obtenerTasasFiltradasPorPais()
            .filter(tasa => tasa.sucursalId === sucursalId)
            .sort((a, b) => Number(a.montoDesdeUsd) - Number(b.montoDesdeUsd));

        if (tasasDelPagador.length === 0) {
            return null;
        }

        const tasasDelRangoVisible = tasasDelPagador.filter(tasa =>
            rangoVisible === "mayor"
                ? Number(tasa.montoDesdeUsd) >= 1000 || tasa.montoHastaUsd === null
                : Number(tasa.montoDesdeUsd) < 1000);

        return tasasDelRangoVisible[0] || tasasDelPagador[0];
    }

    function obtenerTasaParaEntradaDestino() {
        const resultadoActual = contenedorResultado.querySelector("[data-resultado-exitoso='true']");
        const tasaResultado = Number(resultadoActual?.dataset.resultadoTasa || 0);
        if (Number.isFinite(tasaResultado) && tasaResultado > 0) {
            return {
                tasaCambio: tasaResultado,
                codigoMoneda: resultadoActual.dataset.resultadoMoneda || obtenerMonedaDestinoActual(),
                montoRecibe: Number(resultadoActual.dataset.resultadoMontoRecibe || 0)
            };
        }

        const tasaPantalla = obtenerTasaAplicableEnPantalla();
        if (tasaPantalla) {
            return {
                tasaCambio: Number(tasaPantalla.tasaCambio),
                codigoMoneda: tasaPantalla.codigoMoneda,
                montoRecibe: obtenerMontoActual() * Number(tasaPantalla.tasaCambio)
            };
        }

        const tasaReferencia = obtenerTasaReferenciaPorPagador();
        return tasaReferencia
            ? {
                tasaCambio: Number(tasaReferencia.tasaCambio),
                codigoMoneda: tasaReferencia.codigoMoneda,
                montoRecibe: 0
            }
            : null;
    }

    function sincronizarCampoEntrada() {
        if (!campoMontoVisible || !botonMonedaEntrada) {
            return;
        }

        if (modoMonedaEntrada === "destino") {
            const tasaDestino = obtenerTasaParaEntradaDestino();
            const monedaDestino = tasaDestino?.codigoMoneda || obtenerMonedaDestinoActual();
            botonMonedaEntrada.textContent = monedaDestino;
            if (leyendaMonedaEntrada) {
                leyendaMonedaEntrada.textContent = `Toca la moneda para volver a USD o escribe directamente en ${monedaDestino}.`;
            }

            if (tasaDestino && Number.isFinite(tasaDestino.tasaCambio) && tasaDestino.tasaCambio > 0) {
                const montoDestino = Number.isFinite(tasaDestino.montoRecibe) && tasaDestino.montoRecibe > 0
                    ? tasaDestino.montoRecibe
                    : obtenerMontoActual() * tasaDestino.tasaCambio;
                campoMontoVisible.value = montoDestino > 0 ? montoDestino.toFixed(2) : "";
            } else {
                campoMontoVisible.value = "";
            }

            return;
        }

        botonMonedaEntrada.textContent = "USD";
        if (leyendaMonedaEntrada) {
            leyendaMonedaEntrada.textContent = "Toca la moneda para alternar entre USD y la moneda del pais destino.";
        }
        campoMontoVisible.value = campoMonto.value || "";
    }

    function actualizarEstadoPestanas() {
        [pestanaRangoMenor, pestanaRangoMayor].forEach(boton => {
            if (!boton) {
                return;
            }

            const esActivo = boton.dataset.rango === rangoVisible;
            boton.classList.toggle("bg-white", esActivo);
            boton.classList.toggle("text-tinta", esActivo);
            boton.classList.toggle("shadow-sm", esActivo);
            boton.classList.toggle("text-slate-500", !esActivo);
        });
    }

    function renderizarTasas() {
        const tasasPais = obtenerTasasFiltradasPorPais();
        const tasaAplicable = obtenerTasaAplicableEnPantalla();
        const sucursalSeleccionada = obtenerSucursalSeleccionada();
        const menores = tasasPais.filter(tasa => Number(tasa.montoDesdeUsd) < 1000);
        const mayores = tasasPais.filter(tasa => Number(tasa.montoDesdeUsd) >= 1000 || tasa.montoHastaUsd === null);
        const nombrePais = selectorPais.options[selectorPais.selectedIndex]?.text || "el pais seleccionado";
        const tasasDelRango = rangoVisible === "mayor" ? mayores : menores;
        const tasasOrdenadas = [...tasasDelRango].sort((a, b) => {
            const prioridadA = tasaAplicable != null && a.sucursalId === tasaAplicable.sucursalId && a.montoDesdeUsd === tasaAplicable.montoDesdeUsd
                ? 0
                : a.sucursalId === sucursalSeleccionada ? 1 : 2;
            const prioridadB = tasaAplicable != null && b.sucursalId === tasaAplicable.sucursalId && b.montoDesdeUsd === tasaAplicable.montoDesdeUsd
                ? 0
                : b.sucursalId === sucursalSeleccionada ? 1 : 2;

            if (prioridadA !== prioridadB) {
                return prioridadA - prioridadB;
            }

            if (Number(a.montoDesdeUsd) !== Number(b.montoDesdeUsd)) {
                return Number(a.montoDesdeUsd) - Number(b.montoDesdeUsd);
            }

            return a.nombreSucursal.localeCompare(b.nombreSucursal, "es");
        });

        descripcionTasas.textContent = `Tasas vigentes para ${nombrePais}.`;
        tituloListadoTasas.textContent = rangoVisible === "mayor"
            ? "Montos mayores a $1000"
            : "Montos menores a $1000";
        actualizarEstadoPestanas();

        contenedorListaTasas.innerHTML = tasasOrdenadas.length
            ? tasasOrdenadas.map(tasa => crearFilaTasaCompacta(
                tasa,
                tasaAplicable != null && tasa.sucursalId === tasaAplicable.sucursalId && tasa.montoDesdeUsd === tasaAplicable.montoDesdeUsd)).join("")
            : "<p class='rounded-3xl border border-borde bg-white p-4 text-sm text-slate-500'>No hay tasas configuradas para este rango.</p>";
    }

    async function cargarTasasDelCliente() {
        if (!urlTasas) {
            renderizarTasas();
            return;
        }

        const fechaCliente = establecerFechaCliente();
        const parametros = new URLSearchParams({ fechaCliente });
        try {
            const respuesta = await fetch(`${urlTasas}?${parametros.toString()}`, {
                headers: { "X-Requested-With": "XMLHttpRequest" }
            });

            if (respuesta.ok) {
                tasas = await respuesta.json();
            }
        } catch {
            tasas = [];
        }

        renderizarTasas();
        sincronizarCampoEntrada();
    }

    function cargarSucursales(esCargaInicial) {
        const paisId = obtenerPaisSeleccionado();
        selectorSucursal.innerHTML = "";

        const opcionInicial = document.createElement("option");
        opcionInicial.value = "";
        opcionInicial.textContent = paisId ? "Selecciona el pagador" : "Selecciona primero un pais";
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
        sincronizarCampoEntrada();
    }

    async function calcularCotizacion() {
        const paisId = obtenerPaisSeleccionado();
        const sucursalId = obtenerSucursalSeleccionada();
        const monto = obtenerMontoActual();
        const claveActual = obtenerClaveSolicitudActual();

        renderizarTasas();

        if (!paisId || !sucursalId || monto <= 0) {
            ultimaClaveCalculada = "";
            contenedorResultado.innerHTML = "<div class='rounded-[1.75rem] border border-borde bg-white p-5 text-sm leading-7 text-slate-500'>Selecciona pais, canal e ingresa un monto valido para ver tu cotizacion.</div>";
            return;
        }

        if (claveActual === ultimaClaveCalculada) {
            if (debeDesplazarResultado) {
                desplazarResultadoEnMovil();
                debeDesplazarResultado = false;
            }

            return;
        }

        contenedorResultado.innerHTML = "<div class='rounded-[1.75rem] border border-azulMarca/20 bg-azulClaroMarca p-5 text-sm leading-7 text-azulMarca'>Actualizando cotizacion...</div>";

        const respuesta = await fetch(urlCalcular, {
            method: "POST",
            body: new FormData(formulario),
            headers: { "X-Requested-With": "XMLHttpRequest" }
        });

        contenedorResultado.innerHTML = await respuesta.text();
        ultimaClaveCalculada = claveActual;
        sincronizarCampoEntrada();
        if (debeDesplazarResultado) {
            desplazarResultadoEnMovil();
        }

        debeDesplazarResultado = false;
    }

    function programarCalculo(opciones = {}) {
        const { desplazar = false, demora = 280 } = opciones;

        if (temporizadorCalculo !== null) {
            window.clearTimeout(temporizadorCalculo);
        }

        debeDesplazarResultado = desplazar;
        temporizadorCalculo = window.setTimeout(() => {
            calcularCotizacion();
        }, demora);
    }

    establecerFechaCliente();
    selectorPais.value = paisInicial;
    rangoVisible = obtenerRangoSugerido();
    cargarSucursales(true);
    cargarTasasDelCliente();
    sincronizarCampoEntrada();

    selectorPais.addEventListener("change", () => {
        cargarSucursales(false);
        rangoVisible = obtenerRangoSugerido();
        renderizarTasas();
        sincronizarCampoEntrada();
        programarCalculo({ demora: 180 });
    });

    selectorSucursal.addEventListener("change", () => {
        renderizarTasas();
        sincronizarCampoEntrada();
        programarCalculo({ demora: 180 });
    });

    campoMontoVisible.addEventListener("input", () => {
        if (modoMonedaEntrada === "destino") {
            const tasaDestino = obtenerTasaParaEntradaDestino();
            const montoDestino = obtenerMontoVisibleActual();
            if (tasaDestino && Number.isFinite(montoDestino) && montoDestino > 0) {
                // Conserva mayor precision en USD para evitar que al recalcular se degrade
                // el monto destino visible por redondeos intermedios.
                campoMonto.value = (montoDestino / tasaDestino.tasaCambio).toFixed(6);
            } else {
                campoMonto.value = "";
            }
        } else {
            campoMonto.value = campoMontoVisible.value;
        }

        rangoVisible = obtenerRangoSugerido();
        renderizarTasas();
        // Esperar un poco mas evita consultas intermedias mientras el usuario sigue escribiendo.
        programarCalculo({ demora: 900 });
    });

    campoMontoVisible.addEventListener("blur", () => {
        if (modoMonedaEntrada === "usd" && campoMonto.value) {
            campoMontoVisible.value = Number(campoMonto.value).toFixed(2);
        }

        programarCalculo({ desplazar: true, demora: 0 });
    });

    botonMonedaEntrada?.addEventListener("click", () => {
        if (modoMonedaEntrada === "usd" && obtenerMonedaDestinoActual() === "USD") {
            return;
        }

        modoMonedaEntrada = modoMonedaEntrada === "usd" ? "destino" : "usd";
        sincronizarCampoEntrada();
        campoMontoVisible?.focus();
        campoMontoVisible?.select();
    });

    botonEjemplo?.addEventListener("click", () => {
        campoMonto.value = "1250";
        rangoVisible = "mayor";
        renderizarTasas();
        sincronizarCampoEntrada();
        programarCalculo({ desplazar: true, demora: 0 });
    });

    pestanaRangoMenor?.addEventListener("click", () => {
        rangoVisible = "menor";
        renderizarTasas();
    });

    pestanaRangoMayor?.addEventListener("click", () => {
        rangoVisible = "mayor";
        renderizarTasas();
    });

    formulario.addEventListener("submit", async evento => {
        evento.preventDefault();
        await calcularCotizacion();
    });

    renderizarTasas();
    if (campoMonto.value) {
        sincronizarCampoEntrada();
        programarCalculo({ demora: 180 });
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
