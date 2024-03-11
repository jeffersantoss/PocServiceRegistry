const express = require('express');
const Consul = require('consul');
const app = express();
 
app.get('/health', (req, res) => {
    res.status(200).json({ status: "Running" });
});
 
app.get('/api/reviews', (req, res) => {
    res.status(200).json(data);
});
 
 
const serviceName = "review-service-api";
const servicePort = 443;
const ngrokHost = "59f1-201-77-175-11.ngrok-free.app"
const PORT = process.env.PORT || 3000;

app.listen(PORT, () => {
    console.log(`Servidor rodando na porta ${PORT}`);
 
    const consul = new Consul({
        host: "consul.jeffs.dev",
        port: 4433,
        secure: true,
    });
 
    deregisterConsul(consul);
    registerConsul(consul);
 
});


const registerConsul = (consulInstance) => {
    consulInstance.agent.service.register({
        node: ngrokHost,
        name: serviceName,
        address: ngrokHost,
        port: servicePort,
    }, (err, result) => {
        if (err) {
            console.error("Erro ao registrar serviço:", err);
        } else {
            console.log("Serviço registrado com sucesso:", result);
        }
    });
}

const deregisterConsul = (consulInstance) => {
    try{
        consulInstance.agent.service.deregister({
            id: serviceName,
   
        }, (err, result) => {
            if (err) {
                console.error("Erro ao registrar serviço:", err);
            } else {
                console.log("Serviço registrado com sucesso:", result);
            }
        });
    }catch(error){
        let message = 'erro ao desregistrar serviço'
        if (error.message){
            message = error.message
            console.log(message)
        }
        console.log(message)
    }
}

const data = {
    "reviews": [
      {"id": 1, "book_id": 1, "rating": 5, "comment": "Obra-prima inesquecível."},
      {"id": 2, "book_id": 2, "rating": 4, "comment": "Fantástico, mas com final previsível."},
      {"id": 3, "book_id": 3, "rating": 3, "comment": "Interessante, porém longo demais."},
      {"id": 4, "book_id": 4, "rating": 5, "comment": "Incrível, uma jornada emocionante."},
      {"id": 5, "book_id": 5, "rating": 2, "comment": "Decepcionante e arrastado."},
      {"id": 6, "book_id": 6, "rating": 5, "comment": "Perfeitamente pesquisado, um clássico."},
      {"id": 7, "book_id": 7, "rating": 4, "comment": "Muito bom, recomendo."},
      {"id": 8, "book_id": 8, "rating": 1, "comment": "Não consegui terminar, muito chato."},
      {"id": 9, "book_id": 9, "rating": 5, "comment": "Uma obra de arte do começo ao fim."},
      {"id": 10, "book_id": 10, "rating": 3, "comment": "Bom, mas não atendeu às expectativas."}
    ]
  }