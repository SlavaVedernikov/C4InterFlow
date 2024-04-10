import React, { useState, useEffect } from 'react';
import axios from 'axios';
import Split from 'react-split';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { oneLight as PrismStyle} from 'react-syntax-highlighter/dist/esm/styles/prism';
import classNames from 'classnames';
import './App.css';

// TreeView Component
const TreeView = ({ sitemap, onNodeSelect, levelOfDetail, setLevelOfDetail, type, setType, format, setFormat }) => {
  const [expandedNodes, setExpandedNodes] = useState({});
  const [selectedNode, setSelectedNode] = useState(null);

  const toggleNode = (node) => {
    setExpandedNodes({
      ...expandedNodes,
      [node.loc]: !expandedNodes[node.loc]
    });
    setSelectedNode(node);
    onNodeSelect(node);
    if (node.levelsOfDetails && !node.levelsOfDetails.includes(levelOfDetail)) {
      setLevelOfDetail(node.levelsOfDetails[0]);
    }
    if (node.types && !node.types.includes(type)) {
      setType(node.types[0]);
    }
    if (node.formats && !node.formats.includes(format)) {
      setFormat(node.formats[0]);
    }
  };

  const renderNode = (node) => (
    <li key={node.loc}>
      <button className={classNames('node', { 'selected': node === selectedNode })} onClick={() => toggleNode(node)}>
        {node.urlset && <span className={classNames('chevron', { 'expanded': expandedNodes[node.loc] })} />}
        {node.label}
      </button>
      {expandedNodes[node.loc] && node.urlset && <ul>{node.urlset.map(renderNode)}</ul>}
    </li>
  );

  return sitemap ? <ul className="tree-view">{sitemap.urlset.map(renderNode)}</ul> : null;
};



// Controls Component
const Controls = ({ selectedNode, levelOfDetail, setLevelOfDetail, type, setType, format, setFormat }) => {
  if (!selectedNode || !selectedNode.levelsOfDetails || !selectedNode.types || !selectedNode.formats) return null;

  return (
    <div className="controls-container">
      <fieldset>
        <legend>Level of Details</legend>
        {selectedNode.levelsOfDetails.map((x) => (
          <label key={x}>
            <input type="radio" name="levelOfDetail" value={x} checked={x === levelOfDetail} onChange={() => setLevelOfDetail(x)} />
            {x}
          </label>
        ))}
      </fieldset>
      <fieldset>
        <legend>Type</legend>
        {selectedNode.types.map((x) => (
          <label key={x}>
            <input type="radio" name="type" value={x} checked={x === type} onChange={() => setType(x)} />
            {x}
          </label>
        ))}
      </fieldset>
      <fieldset>
        <legend>Format</legend>
        {selectedNode.formats.map((x) => (
          <label key={x}>
            <input type="radio" name="format" value={x} checked={x === format} onChange={() => setFormat(x)} />
            {x}
          </label>
        ))}
      </fieldset>
    </div>
  );
};



// DiagramView Component
const DiagramView = ({ selectedNode, levelOfDetail, type, format }) => {
  const [plantUMLCode, setPlantUMLCode] = useState('');


  useEffect(() => {
    if (format === 'puml' && selectedNode.levelsOfDetails && selectedNode.types && selectedNode.formats) {
      axios.get(diagramUrl)
        .then(response => setPlantUMLCode(response.data))
        .catch(error => console.error('Error fetching PlantUML file:', error));
    }
  }, [selectedNode, levelOfDetail, type, format]);

  if (!selectedNode || !selectedNode.levelsOfDetails || !selectedNode.types || !selectedNode.formats) return null;

  const diagramUrl = `${selectedNode.loc}/${levelOfDetail} - ${type}.${format}`;

  const renderDiagram = () => {
    if (format === 'png' || format === 'svg') {
      return <img src={diagramUrl} alt="Diagram" />;
    } else if (format === 'puml') {
      return <SyntaxHighlighter language="yang" style={PrismStyle} showLineNumbers="true">{plantUMLCode}</SyntaxHighlighter>;
    }
  };

  return (
    <div className="diagrams-container">
      <h2>Diagram</h2>
      {renderDiagram()}
    </div>
  );
};


// Main App Component
const App = () => {
  const [sitemap, setSitemap] = useState(null);
  const [selectedNode, setSelectedNode] = useState(null);
  const [levelOfDetail, setLevelOfDetail] = useState('');
  const [type, setType] = useState('');
  const [format, setFormat] = useState('');

  useEffect(() => {
    // Fetch the sitemap.json on component mount
    axios.get('sitemap.json')
      .then(response => {
        // Select the first top-level node
        const node = response.data.urlset[0];
        setSitemap(response.data);
        setSelectedNode(node);
        if (node.levelsOfDetails && node.types && node.formats) {
          setLevelOfDetail(node.levelsOfDetails[0]);
          setType(node.types[0]);
          setFormat(node.formats[0]);
        }
      })
      .catch(error => console.error('Error fetching sitemap.json:', error));
  }, []);

  return (
    <Split sizes={[25, 75]} minSize={100} expandToMin={false} gutterSize={5} gutterAlign="center" snapOffset={30} dragInterval={1} direction="horizontal" className="split-pane" gutterClassName="gutter">
      <div className="pane pane-left">
        <TreeView sitemap={sitemap} onNodeSelect={setSelectedNode} levelOfDetail={levelOfDetail} setLevelOfDetail={setLevelOfDetail} type={type} setType={setType} format={format} setFormat={setFormat} />
      </div>
      <div className="pane">
        <Controls selectedNode={selectedNode} levelOfDetail={levelOfDetail} setLevelOfDetail={setLevelOfDetail} type={type} setType={setType} format={format} setFormat={setFormat} />
        <DiagramView selectedNode={selectedNode} levelOfDetail={levelOfDetail} type={type} format={format} />
      </div>
    </Split>
  );
};

export default App;
