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

function solicitarJson(url) {
    if (typeof fetch === "function") {
        return fetch(url, {
            headers: { "X-Requested-With": "XMLHttpRequest" }
        }).then(respuesta => {
            if (!respuesta.ok) {
                throw new Error("Respuesta no valida");
            }

            return respuesta.json();
        });
    }

    return new Promise((resolve, reject) => {
        const solicitud = new XMLHttpRequest();
        solicitud.open("GET", url, true);
        solicitud.setRequestHeader("X-Requested-With", "XMLHttpRequest");
        solicitud.onreadystatechange = () => {
            if (solicitud.readyState !== 4) {
                return;
            }

            if (solicitud.status < 200 || solicitud.status >= 300) {
                reject(new Error("Respuesta no valida"));
                return;
            }

            try {
                resolve(JSON.parse(solicitud.responseText || "[]"));
            } catch {
                reject(new Error("Respuesta JSON no valida"));
            }
        };
        solicitud.onerror = () => reject(new Error("No se pudo completar la solicitud"));
        solicitud.send();
    });
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

function inicializarActualizacionPublicidadSvg() {
    const boton = document.getElementById("btnActualizarPublicidadSvg");
    if (!boton) {
        return;
    }

    const texto = boton.querySelector("[data-texto]");
    const loader = boton.querySelector("[data-loader]");
    const url = boton.dataset.url;
    const archivosPublicidadSvg = [
        { titulo: "Publicidad Mexico", url: "/uploads/publicidad/tasas.svg" },
        { titulo: "Historia", url: "/uploads/publicidad/tasas-post.svg", permiteAbrir: false, permiteDescargaPng: true, nombreDescarga: "tasas-post.png" }
    ];

    function obtenerTokenAntifalsificacion() {
        return document.querySelector("input[name='__RequestVerificationToken']")?.value || "";
    }

    function obtenerFechaTasaSeleccionada() {
        return document.getElementById("fechaFiltro")?.value || boton.dataset.fechaTasa || "";
    }

    function establecerCargando(estaCargando) {
        boton.disabled = estaCargando;
        loader?.classList.toggle("hidden", !estaCargando);
        if (texto) {
            texto.textContent = estaCargando ? "Actualizando..." : "Actualizar Publicidad SVG";
        }
    }

    function obtenerClasesBotonPrincipal() {
        return "inline-flex items-center justify-center rounded-xl bg-tinta px-5 py-3 text-sm font-bold text-white transition hover:bg-azulMarca";
    }

    function obtenerClasesBotonSecundario() {
        return "inline-flex items-center justify-center rounded-xl border border-slate-200 bg-white px-5 py-3 text-sm font-bold text-slate-700 transition hover:bg-slate-50";
    }

    async function mostrarAlerta(opciones) {
        if (window.Swal?.fire) {
            return window.Swal.fire(opciones);
        }

        if (opciones.showCancelButton) {
            return { isConfirmed: window.confirm(opciones.text || opciones.title || "Confirmar accion") };
        }

        window.alert(opciones.text || opciones.title || "");
        return { isConfirmed: true };
    }

    async function cargarImagen(url) {
        return new Promise((resolve, reject) => {
            const imagen = new Image();
            imagen.crossOrigin = "anonymous";
            imagen.onload = () => resolve(imagen);
            imagen.onerror = () => reject(new Error("No se pudo cargar el SVG para descargarlo."));
            imagen.src = url;
        });
    }

    function descargarBlob(blob, nombreArchivo) {
        const urlObjeto = URL.createObjectURL(blob);
        const enlace = document.createElement("a");
        enlace.href = urlObjeto;
        enlace.download = nombreArchivo;
        document.body.appendChild(enlace);
        enlace.click();
        enlace.remove();
        window.setTimeout(() => URL.revokeObjectURL(urlObjeto), 1000);
    }

    async function descargarSvgComoPng(urlSvg, nombreArchivo) {
        const imagen = await cargarImagen(urlSvg);
        const ancho = Math.max(imagen.naturalWidth || imagen.width || 0, 1);
        const alto = Math.max(imagen.naturalHeight || imagen.height || 0, 1);
        const lienzo = document.createElement("canvas");
        lienzo.width = ancho;
        lienzo.height = alto;
        const contexto = lienzo.getContext("2d");
        contexto.drawImage(imagen, 0, 0, ancho, alto);

        const blob = await new Promise(resolve => lienzo.toBlob(resolve, "image/png", 1));
        if (!blob) {
            throw new Error("No se pudo generar el archivo PNG.");
        }

        descargarBlob(blob, nombreArchivo);
    }

    async function mostrarVistaPreviaPublicidadSvg() {
        const marcaTiempo = new Date().getTime();
        const vistasPrevias = archivosPublicidadSvg.map(archivo => ({
            ...archivo,
            urlVistaPrevia: `${archivo.url}?v=${marcaTiempo}`
        }));

        if (!window.Swal?.fire) {
            vistasPrevias.forEach(archivo => window.open(archivo.urlVistaPrevia, "_blank"));
            return;
        }

        await window.Swal.fire({
            title: "Vista previa",
            width: "90%",
            html: `
                <div class="grid gap-4 md:grid-cols-2">
                    ${vistasPrevias.map((archivo, indice) => `
                        <section class="min-w-0">
                            <h3 class="mb-2 text-center text-sm font-black uppercase tracking-[0.18em] text-slate-600">${archivo.titulo}</h3>
                            <div class="relative overflow-hidden rounded-2xl border border-slate-200 bg-slate-50" style="height:min(620px, 70vh);">
                                <div id="loaderVistaPreviaPublicidadSvg${indice}" class="absolute inset-0 flex flex-col items-center justify-center gap-3 bg-white text-sm font-semibold text-slate-600">
                                    <span class="h-8 w-8 animate-spin rounded-full border-4 border-azulMarca/20 border-t-azulMarca"></span>
                                    <span>Cargando vista previa...</span>
                                </div>
                                <img
                                    id="imagenVistaPreviaPublicidadSvg${indice}"
                                    src="${archivo.urlVistaPrevia}"
                                    alt="Vista previa ${archivo.titulo}"
                                    class="h-full w-full object-contain opacity-0 transition"
                                    loading="eager" />
                            </div>
                            ${archivo.permiteAbrir === false ? "" : `
                                <a
                                    href="${archivo.urlVistaPrevia}"
                                    target="_blank"
                                    rel="noopener"
                                    class="mt-3 inline-flex w-full items-center justify-center rounded-xl bg-azulMarca px-5 py-3 text-sm font-bold text-white transition hover:bg-tinta">
                                    Abrir ${archivo.titulo.toLowerCase()} en nueva pestana
                                </a>
                            `}
                            ${archivo.permiteDescargaPng ? `
                                <button
                                    type="button"
                                    data-descargar-png="${indice}"
                                    class="mt-2 inline-flex w-full items-center justify-center rounded-xl bg-emerald-600 px-5 py-3 text-sm font-bold text-white transition hover:bg-emerald-700">
                                    Descargar historia PNG
                                </button>
                            ` : ""}
                        </section>
                    `).join("")}
                </div>
            `,
            showCloseButton: true,
            confirmButtonText: "Cerrar",
            buttonsStyling: false,
            customClass: {
                confirmButton: obtenerClasesBotonPrincipal()
            },
            didOpen: () => {
                vistasPrevias.forEach((_, indice) => {
                    const imagen = document.getElementById(`imagenVistaPreviaPublicidadSvg${indice}`);
                    const loaderPreview = document.getElementById(`loaderVistaPreviaPublicidadSvg${indice}`);
                    imagen?.addEventListener("load", () => {
                        loaderPreview?.classList.add("hidden");
                        imagen.classList.remove("opacity-0");
                    }, { once: true });
                    imagen?.addEventListener("error", () => {
                        if (loaderPreview) {
                            loaderPreview.innerHTML = "<span>No se pudo cargar la vista previa. Usa el boton para abrirla en nueva pestana.</span>";
                        }
                    }, { once: true });
                });

                document.querySelectorAll("[data-descargar-png]").forEach(botonDescarga => {
                    botonDescarga.addEventListener("click", async () => {
                        const indice = Number(botonDescarga.dataset.descargarPng);
                        const archivo = vistasPrevias[indice];
                        if (!archivo) {
                            return;
                        }

                        const textoOriginal = botonDescarga.textContent;
                        botonDescarga.disabled = true;
                        botonDescarga.textContent = "Generando PNG...";

                        try {
                            await descargarSvgComoPng(archivo.urlVistaPrevia, archivo.nombreDescarga || "historia.png");
                            botonDescarga.textContent = "PNG descargado";
                            window.setTimeout(() => {
                                botonDescarga.textContent = textoOriginal;
                                botonDescarga.disabled = false;
                            }, 1200);
                        } catch {
                            botonDescarga.textContent = "No se pudo descargar";
                            window.setTimeout(() => {
                                botonDescarga.textContent = textoOriginal;
                                botonDescarga.disabled = false;
                            }, 1800);
                        }
                    });
                });
            }
        });
    }

    boton.addEventListener("click", async () => {
        const confirmacion = await mostrarAlerta({
            title: "Actualizar publicidad",
            text: "Se reemplazaran las tasas de la publicidad y de la historia con las tasas vigentes mas recientes.",
            icon: "question",
            showCancelButton: true,
            confirmButtonText: "Actualizar",
            cancelButtonText: "Cancelar",
            buttonsStyling: false,
            customClass: {
                confirmButton: obtenerClasesBotonPrincipal(),
                cancelButton: obtenerClasesBotonSecundario()
            }
        });

        if (!confirmacion.isConfirmed || !url) {
            return;
        }

        const fechaTasa = obtenerFechaTasaSeleccionada();
        if (!fechaTasa) {
            await mostrarAlerta({
                title: "Fecha requerida",
                text: "Selecciona una fecha para actualizar la publicidad SVG.",
                icon: "warning",
                buttonsStyling: false,
                customClass: {
                    confirmButton: obtenerClasesBotonPrincipal()
                }
            });
            return;
        }

        establecerCargando(true);
        if (window.Swal?.fire) {
            window.Swal.fire({
                title: "Actualizando publicidad",
                text: `Estamos regenerando el SVG con las tasas del ${fechaTasa}.`,
                allowOutsideClick: false,
                allowEscapeKey: false,
                showConfirmButton: false,
                didOpen: () => window.Swal.showLoading()
            });
        }

        try {
            const respuesta = await fetch(url, {
                method: "POST",
                body: new URLSearchParams({ fechaTasa }),
                headers: {
                    "X-Requested-With": "XMLHttpRequest",
                    "RequestVerificationToken": obtenerTokenAntifalsificacion(),
                    "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8"
                }
            });

            const resultado = await respuesta.json();
            await mostrarAlerta({
                title: resultado.success ? "Listo" : "No se pudo actualizar",
                text: resultado.message || "No se pudo completar el proceso.",
                icon: resultado.success ? "success" : "error",
                buttonsStyling: false,
                customClass: {
                    confirmButton: obtenerClasesBotonPrincipal()
                }
            });

            if (resultado.success) {
                await mostrarVistaPreviaPublicidadSvg();
            }
        } catch {
            await mostrarAlerta({
                title: "No se pudo actualizar",
                text: "Ocurrio un error al comunicarse con el servidor.",
                icon: "error",
                buttonsStyling: false,
                customClass: {
                    confirmButton: obtenerClasesBotonPrincipal()
                }
            });
        } finally {
            establecerCargando(false);
        }
    });
}

function inicializarCopiaTasasCambio() {
    const formulario = document.getElementById("formCopiarTasasCambio");
    if (!formulario) {
        return;
    }

    const selectorTodas = document.getElementById("seleccionarTodasTasasCambio");
    const campoCopiarTodas = document.getElementById("copiarTodasTasasCambio");
    const campoFechaDestino = document.getElementById("fechaDestinoCopiarTasas");
    const campoFechaOrigen = formulario.querySelector("input[name='FechaOrigen']");
    const checkboxes = Array.from(document.querySelectorAll(".seleccion-tasa-cambio"));
    let modoCopia = "seleccionadas";
    let envioConfirmado = false;

    function mostrarMensaje(titulo, texto) {
        if (window.Swal?.fire) {
            window.Swal.fire({
                title: titulo,
                text: texto,
                icon: "warning",
                buttonsStyling: false,
                customClass: {
                    confirmButton: "inline-flex items-center justify-center rounded-xl bg-tinta px-5 py-3 text-sm font-bold text-white transition hover:bg-azulMarca"
                }
            });
            return;
        }

        window.alert(texto || titulo);
    }

    async function confirmarCopia(copiarTodas, totalSeleccionadas) {
        const detalle = copiarTodas
            ? "Se copiaran todas las tasas seleccionadas hacia la fecha destino. Si ya existen tasas con el mismo pais, sucursal y rango, seran sobreescritas."
            : `Se copiaran ${totalSeleccionadas} tasa(s) seleccionada(s) hacia la fecha destino. Si ya existen tasas con el mismo pais, sucursal y rango, seran sobreescritas.`;

        if (window.Swal?.fire) {
            const resultado = await window.Swal.fire({
                title: "Confirmar traslado",
                text: detalle,
                icon: "question",
                showCancelButton: true,
                confirmButtonText: "Si, continuar",
                cancelButtonText: "Cancelar",
                buttonsStyling: false,
                customClass: {
                    confirmButton: "inline-flex items-center justify-center rounded-xl bg-emerald-600 px-5 py-3 text-sm font-bold text-white transition hover:bg-emerald-700",
                    cancelButton: "inline-flex items-center justify-center rounded-xl bg-rose-600 px-5 py-3 text-sm font-bold text-white transition hover:bg-rose-700"
                }
            });

            return resultado.isConfirmed;
        }

        return window.confirm(detalle);
    }

    selectorTodas?.addEventListener("change", () => {
        checkboxes.forEach(checkbox => {
            checkbox.checked = selectorTodas.checked;
        });
    });

    formulario.querySelectorAll("[data-modo-copia]").forEach(boton => {
        boton.addEventListener("click", () => {
            modoCopia = boton.dataset.modoCopia || "seleccionadas";
        });
    });

    formulario.addEventListener("submit", async evento => {
        if (envioConfirmado) {
            envioConfirmado = false;
            return;
        }

        evento.preventDefault();

        const boton = evento.submitter;
        if (boton?.dataset.modoCopia) {
            modoCopia = boton.dataset.modoCopia;
        }

        const copiarTodas = modoCopia === "todas";
        const totalSeleccionadas = checkboxes.filter(checkbox => checkbox.checked).length;
        campoCopiarTodas.value = copiarTodas ? "true" : "false";

        if (!campoFechaDestino?.value) {
            mostrarMensaje("Fecha requerida", "Selecciona la fecha destino para copiar las tasas.");
            return;
        }

        if (copiarTodas && !campoFechaOrigen?.value) {
            mostrarMensaje("Fecha origen requerida", "Filtra por una fecha especifica antes de copiar todas las tasas.");
            return;
        }

        if (!copiarTodas && totalSeleccionadas === 0) {
            mostrarMensaje("Seleccion requerida", "Selecciona al menos una tasa para copiar.");
            return;
        }

        if (await confirmarCopia(copiarTodas, totalSeleccionadas)) {
            envioConfirmado = true;
            formulario.requestSubmit(boton);
        }
    });
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

function inicializarFormularioPublicidad() {
    const contenedorPreview = document.getElementById("previewPublicidad");
    const campoArchivo = document.getElementById("archivoPublicidad");
    const selectorTipo = document.getElementById("tipoRecursoPublicidad");
    if (!contenedorPreview || !campoArchivo) {
        return;
    }

    let urlObjetoActual = null;

    function limpiarUrlObjeto() {
        if (urlObjetoActual) {
            URL.revokeObjectURL(urlObjetoActual);
            urlObjetoActual = null;
        }
    }

    function mostrarVacio() {
        contenedorPreview.innerHTML = "<p class='px-5 text-center text-sm leading-6 text-slate-500'>Selecciona un archivo para previsualizarlo.</p>";
    }

    function mostrarRecurso(url, tipo) {
        const esVideo = String(tipo || "").toLowerCase().includes("video") || /\.(mp4|webm|mov|m4v|avi|mpeg|mpg)$/i.test(url);
        contenedorPreview.innerHTML = esVideo
            ? `<video src="${url}" class="max-h-80 w-full bg-black object-contain" controls muted preload="metadata"></video>`
            : `<img src="${url}" alt="Previsualizacion de publicidad" class="max-h-80 w-full object-contain" loading="lazy" />`;
    }

    const urlActual = contenedorPreview.dataset.urlActual;
    if (urlActual) {
        mostrarRecurso(urlActual, contenedorPreview.dataset.tipoActual);
    } else {
        mostrarVacio();
    }

    campoArchivo.addEventListener("change", () => {
        const archivo = campoArchivo.files?.[0];
        if (!archivo) {
            limpiarUrlObjeto();
            if (urlActual) {
                mostrarRecurso(urlActual, contenedorPreview.dataset.tipoActual);
            } else {
                mostrarVacio();
            }
            return;
        }

        limpiarUrlObjeto();
        urlObjetoActual = URL.createObjectURL(archivo);
        const tipo = archivo.type.startsWith("video/") ? "Video" : selectorTipo?.options[selectorTipo.selectedIndex]?.text || "Imagen";
        mostrarRecurso(urlObjetoActual, tipo);
    });
}

function inicializarPublicidadPublica() {
    const modulo = document.getElementById("modulo-publicidad");
    if (!modulo) {
        return;
    }

    const visor = document.getElementById("publicidad-visor");
    const estado = document.getElementById("publicidad-estado");
    const titulo = document.getElementById("publicidad-titulo");
    const descripcion = document.getElementById("publicidad-descripcion");
    const contador = document.getElementById("publicidad-contador");
    const botonPantallaCompleta = document.getElementById("botonPantallaCompletaPublicidad");
    const urlPublicidad = modulo.dataset.urlPublicidad;
    const esModoPantalla = modulo.dataset.modoPantalla === "true";
    const repetirVideos = modulo.dataset.loopVideos === "true";
    let publicidades = [];
    let indiceActual = 0;
    let temporizador = null;
    let recursoActual = null;
    let recursoPrecargado = null;

    function limpiarTemporizador() {
        if (temporizador !== null) {
            window.clearTimeout(temporizador);
            temporizador = null;
        }
    }

    function mostrarEstado(mensaje) {
        if (esModoPantalla || !estado) {
            return;
        }

        estado.textContent = mensaje;
        estado.classList.remove("hidden");
    }

    function ocultarEstado() {
        estado?.classList.add("hidden");
    }

    function avanzar() {
        limpiarTemporizador();
        if (publicidades.length === 0) {
            mostrarEstado("No hay publicidad activa por el momento.");
            return;
        }

        indiceActual = (indiceActual + 1) % publicidades.length;
        mostrarPublicidadActual();
    }

    function precargarSiguiente() {
        if (publicidades.length < 2) {
            return;
        }

        const siguiente = publicidades[(indiceActual + 1) % publicidades.length];
        if (String(siguiente.tipoRecurso).toLowerCase() === "imagen") {
            const imagen = new Image();
            imagen.loading = "lazy";
            imagen.src = siguiente.urlRecurso;
            recursoPrecargado = imagen;
            return;
        }

        const video = document.createElement("video");
        video.muted = true;
        video.playsInline = true;
        video.preload = "metadata";
        video.src = siguiente.urlRecurso;
        recursoPrecargado = video;
    }

    function programarAvance(segundos) {
        limpiarTemporizador();
        const duracionMs = Math.max(Number(segundos || 1), 1) * 1000;
        temporizador = window.setTimeout(avanzar, duracionMs);
    }

    function configurarTexto(publicidad) {
        if (titulo) {
            titulo.textContent = publicidad.titulo || "Publicidad";
        }

        if (descripcion) {
            descripcion.textContent = publicidad.descripcion || "";
        }

        if (contador) {
            contador.textContent = `${indiceActual + 1} / ${publicidades.length}`;
        }
    }

    function manejarErrorCarga() {
        mostrarEstado("No se pudo cargar este recurso. Mostrando el siguiente...");
        programarAvance(1);
    }

    function reemplazarRecurso(recursoNuevo) {
        const recursoAnterior = recursoActual;
        recursoActual = recursoNuevo;
        visor.appendChild(recursoNuevo);

        window.requestAnimationFrame(() => {
            recursoNuevo.classList.add("publicidad-media--visible");
            if (recursoAnterior) {
                recursoAnterior.classList.remove("publicidad-media--visible");
                recursoAnterior.classList.add("publicidad-media--saliente");
            }
        });

        if (recursoAnterior) {
            window.setTimeout(() => {
                if (recursoAnterior.tagName === "VIDEO") {
                    recursoAnterior.pause();
                    recursoAnterior.removeAttribute("src");
                    recursoAnterior.load();
                }

                recursoAnterior.remove();
            }, 760);
        }
    }

    function mostrarImagen(publicidad) {
        const imagen = document.createElement("img");
        imagen.alt = publicidad.titulo || "Publicidad";
        imagen.loading = "lazy";
        imagen.decoding = "async";
        imagen.className = "publicidad-media";
        imagen.addEventListener("load", () => {
            ocultarEstado();
            reemplazarRecurso(imagen);
            programarAvance(publicidad.duracionSegundos);
            precargarSiguiente();
        }, { once: true });
        imagen.addEventListener("error", manejarErrorCarga, { once: true });
        imagen.src = publicidad.urlRecurso;
    }

    function mostrarVideo(publicidad) {
        const video = document.createElement("video");
        video.className = "publicidad-media";
        video.muted = true;
        video.autoplay = true;
        video.playsInline = true;
        video.preload = "metadata";
        video.loop = repetirVideos && publicidades.length === 1;
        video.addEventListener("loadeddata", () => {
            ocultarEstado();
            reemplazarRecurso(video);
            const reproduccion = video.play();
            if (reproduccion?.catch) {
                reproduccion.catch(() => programarAvance(publicidad.duracionSegundos));
            }

            if (!video.loop) {
                programarAvance(publicidad.duracionSegundos);
            }

            precargarSiguiente();
        }, { once: true });
        if (!video.loop) {
            video.addEventListener("ended", avanzar, { once: true });
        }

        video.addEventListener("error", manejarErrorCarga, { once: true });
        video.src = publicidad.urlRecurso;
    }

    function mostrarPublicidadActual() {
        const publicidad = publicidades[indiceActual];
        if (!publicidad) {
            mostrarEstado("No hay publicidad activa por el momento.");
            return;
        }

        mostrarEstado("Cargando publicidad...");
        configurarTexto(publicidad);

        if (String(publicidad.tipoRecurso).toLowerCase() === "video") {
            mostrarVideo(publicidad);
            return;
        }

        mostrarImagen(publicidad);
    }

    async function cargarPublicidad() {
        publicidades = obtenerJsonDesdeElemento("datos-publicidad-inicial");

        if (publicidades.length === 0 && urlPublicidad) {
            try {
                publicidades = await solicitarJson(urlPublicidad);
            } catch {
                publicidades = [];
            }
        }

        if (publicidades.length === 0) {
            mostrarEstado("No hay publicidad activa por el momento.");
            return;
        }

        mostrarPublicidadActual();
    }

    botonPantallaCompleta?.addEventListener("click", async () => {
        try {
            if (document.fullscreenElement) {
                await document.exitFullscreen();
                return;
            }

            await modulo.requestFullscreen();
        } catch {
            botonPantallaCompleta.style.display = "none";
        }
    });

    cargarPublicidad();
}

document.addEventListener("DOMContentLoaded", () => {
    inicializarCalculadoraPublica();
    inicializarFormularioTasaCambio();
    inicializarActualizacionPublicidadSvg();
    inicializarCopiaTasasCambio();
    inicializarFormularioPublicidad();
    inicializarPublicidadPublica();
    inicializarToasts();
});
